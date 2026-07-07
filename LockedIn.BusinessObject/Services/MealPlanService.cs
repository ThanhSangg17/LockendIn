using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.DTOs.MealPlans;
using LockedIn.BusinessObject.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace LockedIn.BusinessObject.Services;

public class MealPlanService : IMealPlanService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGeminiService _geminiService;
    private readonly ILogger<MealPlanService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;

    public MealPlanService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IGeminiService geminiService,
        ILogger<MealPlanService> logger,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _geminiService = geminiService;
        _logger = logger;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
    }

    public async Task<ApiResponse<MealPlanResponse>> GenerateMealPlanAsync(GenerateMealPlanRequest request)
    {
        var pt = await GetCurrentPtProfileAsync();
        if (pt == null)
            return ApiResponse<MealPlanResponse>.Fail("Only personal trainers can generate AI meal plans.");

        var workspace = await _unitOfWork.Workspaces.Query().FirstOrDefaultAsync(w => w.Id == request.WorkspaceId);
        if (workspace == null)
            return ApiResponse<MealPlanResponse>.Fail("Workspace not found.");

        if (workspace.PtProfileId != pt.Id)
            return ApiResponse<MealPlanResponse>.Fail("You are not the personal trainer assigned to this workspace.");

        if (request.GenerationRequestId == null || request.GenerationRequestId == Guid.Empty)
        {
            request.GenerationRequestId = Guid.NewGuid();
            _logger.LogInformation("No GenerationRequestId provided. Generated new: {Id}", request.GenerationRequestId);
        }

        AddonQuotaReservation? reservedQuota = null;
        bool reserveSuccess = false;
        int dailyLimit = _configuration.GetValue<int>("AIQuota:DailyMealPlanGenerationLimit", 20);
        int leaseMinutes = _configuration.GetValue<int>("AIQuota:MealPlanReservationLeaseMinutes", 5);

        try
        {
            await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            
            await OpportunisticCleanupAsync(pt.Id, _unitOfWork);

            var existingReservation = await _unitOfWork.AddonQuotaReservations.Query()
                .FirstOrDefaultAsync(r => r.GenerationRequestId == request.GenerationRequestId);

            if (existingReservation != null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                if (existingReservation.Status == (int)AddonQuotaReservationStatus.Pending)
                    return ApiResponse<MealPlanResponse>.Fail("Request is being processed.");
                if (existingReservation.Status == (int)AddonQuotaReservationStatus.Finalized)
                    return ApiResponse<MealPlanResponse>.Ok(null!, "Meal Plan đã được tạo thành công. Vui lòng refresh lại danh sách.");
                if (existingReservation.Status == (int)AddonQuotaReservationStatus.Released)
                    return ApiResponse<MealPlanResponse>.Fail("Previous request failed. Please generate a new request.");
                return ApiResponse<MealPlanResponse>.Fail("Invalid request state.");
            }

            var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);
            var counter = await _unitOfWork.MealPlanQuotaCounters.Query()
                .FirstOrDefaultAsync(c => c.PtProfileId == pt.Id && c.QuotaDate == todayUtc);

            if (counter == null)
            {
                counter = new MealPlanQuotaCounter
                {
                    Id = Guid.NewGuid(),
                    PtProfileId = pt.Id,
                    QuotaDate = todayUtc,
                    DailyLimit = dailyLimit,
                    ConsumedCount = 0,
                    ReservedCount = 0,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.MealPlanQuotaCounters.AddAsync(counter);
            }

            if (counter.ConsumedCount + counter.ReservedCount < counter.DailyLimit)
            {
                counter.ReservedCount++;
                reservedQuota = new AddonQuotaReservation
                {
                    Id = Guid.NewGuid(),
                    PtProfileId = pt.Id,
                    GenerationRequestId = request.GenerationRequestId.Value,
                    ReservationType = "FreeQuota",
                    QuotaDate = todayUtc,
                    Status = (int)AddonQuotaReservationStatus.Pending,
                    ReservedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(leaseMinutes)
                };
                await _unitOfWork.AddonQuotaReservations.AddAsync(reservedQuota);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                reserveSuccess = true;
            }
            else
            {
                var credit = await _unitOfWork.AddonEntitlements.Query()
                    .Where(e => e.PtProfileId == pt.Id 
                             && e.ProductCode == "MEAL_PLAN_CREDIT"
                             && e.FulfillmentType == "Credit"
                             && e.Status == (int)AddonEntitlementStatus.Active
                             && e.QuantityRemaining > 0
                             && (e.ExpiresAt == null || e.ExpiresAt > DateTime.UtcNow))
                    .OrderBy(e => e.ExpiresAt.HasValue ? 0 : 1)
                    .ThenBy(e => e.ExpiresAt)
                    .ThenBy(e => e.CreatedAt)
                    .ThenBy(e => e.Id)
                    .FirstOrDefaultAsync();

                if (credit != null)
                {
                    credit.QuantityRemaining--;
                    reservedQuota = new AddonQuotaReservation
                    {
                        Id = Guid.NewGuid(),
                        PtProfileId = pt.Id,
                        EntitlementId = credit.Id,
                        GenerationRequestId = request.GenerationRequestId.Value,
                        ReservationType = "Credit",
                        Status = (int)AddonQuotaReservationStatus.Pending,
                        ReservedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(leaseMinutes)
                    };
                    await _unitOfWork.AddonQuotaReservations.AddAsync(reservedQuota);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    reserveSuccess = true;
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<MealPlanResponse>.Fail("Daily AI generation quota exceeded and no Meal Plan Credits available.");
                }
            }
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Failed to reserve quota/credit.");
            
            using var freshScope = _scopeFactory.CreateScope();
            var freshUow = freshScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var freshRes = await freshUow.AddonQuotaReservations.Query().FirstOrDefaultAsync(r => r.GenerationRequestId == request.GenerationRequestId);
            if (freshRes != null)
            {
                if (freshRes.Status == (int)AddonQuotaReservationStatus.Pending) return ApiResponse<MealPlanResponse>.Fail("Request is being processed.");
                if (freshRes.Status == (int)AddonQuotaReservationStatus.Finalized) return ApiResponse<MealPlanResponse>.Ok(null!, "Meal Plan đã được tạo thành công. Vui lòng refresh lại danh sách.");
            }
            return ApiResponse<MealPlanResponse>.Fail("Concurrency error during reservation. Please try again.");
        }

        string contentJson;
        int tokensUsed = 0;
        try
        {
            var result = await _geminiService.GenerateMealPlanJsonAsync(request);
            contentJson = result.JsonContent;
            tokensUsed = result.TokensUsed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini response fail.");
            await ReleaseReservationAsync(reservedQuota.Id);
            return ApiResponse<MealPlanResponse>.Fail($"Failed to generate meal plan: {ex.Message}");
        }

        try
        {
            using var jsonDoc = System.Text.Json.JsonDocument.Parse(contentJson);
            var root = jsonDoc.RootElement;
            if (!root.TryGetProperty("days", out var daysElement) || daysElement.ValueKind != System.Text.Json.JsonValueKind.Array)
            {
                await ReleaseReservationAsync(reservedQuota.Id);
                return ApiResponse<MealPlanResponse>.Fail("Invalid meal plan structure generated by AI: missing 'days' list.");
            }
            bool hasMeals = false;
            foreach (var dayElement in daysElement.EnumerateArray())
            {
                if (dayElement.TryGetProperty("meals", out var mealsElement) && mealsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    hasMeals = true;
                    break;
                }
            }
            if (!hasMeals)
            {
                await ReleaseReservationAsync(reservedQuota.Id);
                return ApiResponse<MealPlanResponse>.Fail("Invalid meal plan structure generated by AI: 'days' must contain 'meals'.");
            }
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "JSON validation fail: invalid JSON content.");
            await ReleaseReservationAsync(reservedQuota.Id);
            return ApiResponse<MealPlanResponse>.Fail("AI generated invalid JSON format.");
        }

        MealPlan mealPlan;
        try
        {
            using var finalizeScope = _scopeFactory.CreateScope();
            var finalizeUow = finalizeScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await finalizeUow.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            var freshRes = await finalizeUow.AddonQuotaReservations.Query().FirstOrDefaultAsync(r => r.Id == reservedQuota.Id);
            if (freshRes == null || freshRes.Status != (int)AddonQuotaReservationStatus.Pending || freshRes.ExpiresAt < DateTime.UtcNow)
            {
                await finalizeUow.RollbackTransactionAsync();
                return ApiResponse<MealPlanResponse>.Fail("Reservation expired or already processed.");
            }

            if (freshRes.ReservationType == "FreeQuota")
            {
                var counter = await finalizeUow.MealPlanQuotaCounters.Query().FirstOrDefaultAsync(c => c.PtProfileId == pt.Id && c.QuotaDate == freshRes.QuotaDate);
                if (counter != null && counter.ReservedCount > 0)
                {
                    counter.ReservedCount--;
                    counter.ConsumedCount++;
                }
            }
            
            var activePlans = await finalizeUow.MealPlans.Query().Where(x => x.WorkspaceId == workspace.Id && !x.IsDeleted && x.IsActive).ToListAsync();
            foreach (var plan in activePlans)
            {
                plan.IsActive = false;
                finalizeUow.MealPlans.Update(plan);
            }

            mealPlan = new MealPlan
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspace.Id,
                CreatedByPtId = pt.Id,
                Title = "AI Meal Plan",
                ContentJson = contentJson,
                Source = 2,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            var aiLog = new AiUsageLog
            {
                Id = Guid.NewGuid(),
                PtProfileId = pt.Id,
                Feature = "AI_MEAL_PLAN",
                TokenUsed = tokensUsed,
                CreatedAt = DateTime.UtcNow
            };

            freshRes.Status = (int)AddonQuotaReservationStatus.Finalized;
            freshRes.FinalizedAt = DateTime.UtcNow;

            await finalizeUow.MealPlans.AddAsync(mealPlan);
            await finalizeUow.AiUsageLogs.AddAsync(aiLog);
            await finalizeUow.SaveChangesAsync();
            await finalizeUow.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to finalize meal plan save.");
            await ReleaseReservationAsync(reservedQuota.Id);
            return ApiResponse<MealPlanResponse>.Fail("Failed to save generated meal plan.");
        }

        var response = MapToMealPlanResponse(mealPlan);
        return ApiResponse<MealPlanResponse>.Ok(response, "AI meal plan generated successfully.");
    }

    private async Task ReleaseReservationAsync(Guid reservationId)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await uow.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            var r = await uow.AddonQuotaReservations.Query().Include(x => x.Entitlement).FirstOrDefaultAsync(x => x.Id == reservationId);
            if (r != null && r.Status == (int)AddonQuotaReservationStatus.Pending)
            {
                if (r.ReservationType == "FreeQuota")
                {
                    var c = await uow.MealPlanQuotaCounters.Query().FirstOrDefaultAsync(x => x.PtProfileId == r.PtProfileId && x.QuotaDate == r.QuotaDate);
                    if (c != null && c.ReservedCount > 0) c.ReservedCount--;
                }
                else if (r.ReservationType == "Credit" && r.EntitlementId.HasValue && r.Entitlement != null)
                {
                    r.Entitlement.QuantityRemaining++;
                    if (r.Entitlement.QuantityRemaining > r.Entitlement.QuantityGranted) r.Entitlement.QuantityRemaining = r.Entitlement.QuantityGranted;
                    if (r.Entitlement.Status == (int)AddonEntitlementStatus.Exhausted && (r.Entitlement.ExpiresAt == null || r.Entitlement.ExpiresAt > DateTime.UtcNow))
                        r.Entitlement.Status = (int)AddonEntitlementStatus.Active;
                }
                r.Status = (int)AddonQuotaReservationStatus.Released;
                r.ReleasedAt = DateTime.UtcNow;
                await uow.SaveChangesAsync();
                await uow.CommitTransactionAsync();
            }
            else
            {
                await uow.RollbackTransactionAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical failure during ReleaseReservationAsync for {ReservationId}", reservationId);
        }
    }

    public async Task<ApiResponse<MealPlanResponse>> CreateMealPlanAsync(CreateMealPlanRequest request)
    {
        var pt = await GetCurrentPtProfileAsync();
        if (pt == null)
        {
            return ApiResponse<MealPlanResponse>.Fail("Only personal trainers can create meal plans.");
        }

        request.Title = request.Title?.Trim()!;
        request.ContentJson = request.ContentJson?.Trim()!;

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return ApiResponse<MealPlanResponse>.Fail("Title is required.");
        }

        if (request.Title.Length > 150)
        {
            return ApiResponse<MealPlanResponse>.Fail("Title cannot exceed 150 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.ContentJson))
        {
            return ApiResponse<MealPlanResponse>.Fail("ContentJson is required.");
        }

        try
        {
            using var jsonDoc = System.Text.Json.JsonDocument.Parse(request.ContentJson);
        }
        catch (System.Text.Json.JsonException)
        {
            return ApiResponse<MealPlanResponse>.Fail("ContentJson must be a valid JSON string.");
        }

        var workspace = await _unitOfWork.Workspaces.Query()
            .FirstOrDefaultAsync(w => w.Id == request.WorkspaceId);

        if (workspace == null)
        {
            return ApiResponse<MealPlanResponse>.Fail("Workspace not found.");
        }

        if (workspace.PtProfileId != pt.Id)
        {
            return ApiResponse<MealPlanResponse>.Fail("You are not the personal trainer assigned to this workspace.");
        }

        // Set other meal plans in same workspace IsActive = false
        await DeactivateMealPlansInWorkspaceAsync(workspace.Id);

        var mealPlan = new MealPlan
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            CreatedByPtId = pt.Id,
            Title = request.Title,
            ContentJson = request.ContentJson,
            Source = request.Source,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.MealPlans.AddAsync(mealPlan);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToMealPlanResponse(mealPlan);
        return ApiResponse<MealPlanResponse>.Ok(response, "Meal plan created successfully.");
    }

    public async Task<ApiResponse<MealPlanQuotaResponse>> GetQuotaStatusAsync()
    {
        var pt = await GetCurrentPtProfileAsync();
        if (pt == null)
        {
            return ApiResponse<MealPlanQuotaResponse>.Fail("Only personal trainers can view meal plan quota.");
        }

        int dailyLimit = _configuration.GetValue<int>("AIQuota:DailyMealPlanGenerationLimit", 20);
        var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);

        var counter = await _unitOfWork.MealPlanQuotaCounters.Query()
            .FirstOrDefaultAsync(c => c.PtProfileId == pt.Id && c.QuotaDate == todayUtc);

        int used = counter?.ConsumedCount ?? 0;
        int reserved = counter?.ReservedCount ?? 0;
        int currentLimit = counter?.DailyLimit ?? dailyLimit;
        int freeRemaining = currentLimit - used - reserved;

        var credits = await _unitOfWork.AddonEntitlements.Query()
            .Where(e => e.PtProfileId == pt.Id 
                     && e.ProductCode == "MEAL_PLAN_CREDIT"
                     && e.FulfillmentType == "Credit"
                     && e.Status == (int)AddonEntitlementStatus.Active
                     && e.QuantityRemaining > 0
                     && (e.ExpiresAt == null || e.ExpiresAt > DateTime.UtcNow))
            .SumAsync(e => e.QuantityRemaining);

        var response = new MealPlanQuotaResponse
        {
            DailyFreeQuota = currentLimit,
            FreeQuotaUsedToday = used,
            FreeQuotaReservedNow = reserved,
            FreeQuotaRemaining = freeRemaining,
            PaidMealPlanCreditRemaining = credits,
            TotalGenerationsRemaining = freeRemaining + credits,
            UsageDateUtc = todayUtc,
            ServerUtcNow = DateTime.UtcNow
        };

        return ApiResponse<MealPlanQuotaResponse>.Ok(response, "Quota retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<MealPlanResponse>>> GetMealPlansByWorkspaceAsync(Guid workspaceId)
    {
        var workspace = await _unitOfWork.Workspaces.Query()
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
        {
            return ApiResponse<IReadOnlyList<MealPlanResponse>>.Fail("Workspace not found.");
        }

        var (allowed, error) = await CanAccessWorkspaceAsync(workspace);
        if (!allowed)
        {
            return ApiResponse<IReadOnlyList<MealPlanResponse>>.Fail(error ?? "Access denied.");
        }

        var mealPlans = await _unitOfWork.MealPlans.Query()
            .Where(mp => mp.WorkspaceId == workspaceId && !mp.IsDeleted)
            .OrderByDescending(mp => mp.CreatedAt)
            .ToListAsync();

        var response = mealPlans.Select(MapToMealPlanResponse).ToList();
        return ApiResponse<IReadOnlyList<MealPlanResponse>>.Ok(response, "Meal plans retrieved successfully.");
    }

    public async Task<ApiResponse<MealPlanResponse>> GetMealPlanByIdAsync(Guid mealPlanId)
    {
        var mealPlan = await _unitOfWork.MealPlans.Query()
            .Include(mp => mp.Workspace)
            .FirstOrDefaultAsync(mp => mp.Id == mealPlanId && !mp.IsDeleted);

        if (mealPlan == null)
        {
            return ApiResponse<MealPlanResponse>.Fail("Meal plan not found.");
        }

        var (allowed, error) = await CanAccessWorkspaceAsync(mealPlan.Workspace);
        if (!allowed)
        {
            return ApiResponse<MealPlanResponse>.Fail(error ?? "Access denied.");
        }

        var response = MapToMealPlanResponse(mealPlan);
        return ApiResponse<MealPlanResponse>.Ok(response, "Meal plan retrieved successfully.");
    }

    public async Task<ApiResponse<MealPlanResponse>> ActivateMealPlanAsync(Guid mealPlanId)
    {
        var mealPlan = await _unitOfWork.MealPlans.Query()
            .Include(mp => mp.Workspace)
            .FirstOrDefaultAsync(mp => mp.Id == mealPlanId && !mp.IsDeleted);

        if (mealPlan == null)
        {
            return ApiResponse<MealPlanResponse>.Fail("Meal plan not found.");
        }

        var (allowed, error) = await CanManageWorkspaceAsync(mealPlan.Workspace);
        if (!allowed)
        {
            return ApiResponse<MealPlanResponse>.Fail(error ?? "Access denied.");
        }

        // Set all meal plans in same workspace IsActive = false
        await DeactivateMealPlansInWorkspaceAsync(mealPlan.WorkspaceId);

        mealPlan.IsActive = true;
        mealPlan.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.MealPlans.Update(mealPlan);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToMealPlanResponse(mealPlan);
        return ApiResponse<MealPlanResponse>.Ok(response, "Meal plan activated successfully.");
    }

    public async Task<ApiResponse<string>> DeleteMealPlanAsync(Guid mealPlanId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<string>.Fail("User is not authenticated.");
        }

        var mealPlan = await _unitOfWork.MealPlans.Query()
            .Include(mp => mp.Workspace)
            .FirstOrDefaultAsync(mp => mp.Id == mealPlanId && !mp.IsDeleted);

        if (mealPlan == null)
        {
            return ApiResponse<string>.Fail("Meal plan not found.");
        }

        var (allowed, error) = await CanManageWorkspaceAsync(mealPlan.Workspace);
        if (!allowed)
        {
            return ApiResponse<string>.Fail(error ?? "Access denied.");
        }

        mealPlan.IsDeleted = true;
        mealPlan.IsActive = false;
        mealPlan.DeletedAt = DateTime.UtcNow;
        mealPlan.DeletedBy = _currentUserService.UserId.Value;

        _unitOfWork.MealPlans.Update(mealPlan);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<string>.Ok("Meal plan deleted successfully.", "Meal plan deleted successfully.");
    }

    #region Access Control Helpers & Mapping

    private async Task OpportunisticCleanupAsync(Guid ptId, IUnitOfWork innerUow)
    {
        var staleReservations = await innerUow.AddonQuotaReservations.Query()
            .Include(r => r.Entitlement)
            .Where(r => r.PtProfileId == ptId && r.Status == (int)AddonQuotaReservationStatus.Pending && r.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        foreach (var reservation in staleReservations)
        {
            if (reservation.ReservationType == "FreeQuota")
            {
                var counter = await innerUow.MealPlanQuotaCounters.Query()
                    .FirstOrDefaultAsync(c => c.PtProfileId == reservation.PtProfileId && c.QuotaDate == reservation.QuotaDate);
                
                if (counter != null && counter.ReservedCount > 0)
                {
                    counter.ReservedCount--;
                }
            }
            else if (reservation.ReservationType == "Credit" && reservation.EntitlementId.HasValue)
            {
                var entitlement = reservation.Entitlement;
                if (entitlement != null)
                {
                    entitlement.QuantityRemaining++;
                    if (entitlement.QuantityRemaining > entitlement.QuantityGranted)
                    {
                        entitlement.QuantityRemaining = entitlement.QuantityGranted;
                    }
                    if (entitlement.Status == (int)AddonEntitlementStatus.Exhausted && (entitlement.ExpiresAt == null || entitlement.ExpiresAt > DateTime.UtcNow))
                    {
                        entitlement.Status = (int)AddonEntitlementStatus.Active;
                    }
                }
            }
            reservation.Status = (int)AddonQuotaReservationStatus.Released;
            reservation.ReleasedAt = DateTime.UtcNow;
        }
    }

    private async Task<PtProfile?> GetCurrentPtProfileAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            return null;
        if (_currentUserService.Role != (int)UserRole.PersonalTrainer)
            return null;

        var userId = _currentUserService.UserId.Value;
        return await _unitOfWork.PtProfiles.Query()
            .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);
    }

    private async Task<CustomerProfile?> GetCurrentCustomerProfileAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            return null;
        if (_currentUserService.Role != (int)UserRole.Customer)
            return null;

        var userId = _currentUserService.UserId.Value;
        return await _unitOfWork.CustomerProfiles.Query()
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
    }

    private async Task<(bool Allowed, string? Error)> CanAccessWorkspaceAsync(Workspace workspace)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            return (false, "User is not authenticated.");

        if (_currentUserService.Role == (int)UserRole.Admin)
            return (true, null);

        if (_currentUserService.Role == (int)UserRole.Customer)
        {
            var customer = await GetCurrentCustomerProfileAsync();
            if (customer == null || workspace.CustomerId != customer.Id)
                return (false, "Access denied to this workspace.");
        }
        else if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var pt = await GetCurrentPtProfileAsync();
            if (pt == null || workspace.PtProfileId != pt.Id)
                return (false, "Access denied to this workspace.");
        }
        else
        {
            return (false, "Invalid user role.");
        }

        return (true, null);
    }

    private async Task<(bool Allowed, string? Error)> CanManageWorkspaceAsync(Workspace workspace)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            return (false, "User is not authenticated.");

        if (_currentUserService.Role == (int)UserRole.Admin)
            return (true, null);

        if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var pt = await GetCurrentPtProfileAsync();
            if (pt == null || workspace.PtProfileId != pt.Id)
                return (false, "Only the assigned personal trainer can manage meal plans in this workspace.");
        }
        else
        {
            return (false, "Access denied. Only personal trainers or admins can manage meal plans.");
        }

        return (true, null);
    }

    private async Task DeactivateMealPlansInWorkspaceAsync(Guid workspaceId)
    {
        var activePlans = await _unitOfWork.MealPlans.Query()
            .Where(x => x.WorkspaceId == workspaceId && !x.IsDeleted && x.IsActive)
            .ToListAsync();

        foreach (var plan in activePlans)
        {
            plan.IsActive = false;
            _unitOfWork.MealPlans.Update(plan);
        }
    }

    private MealPlanResponse MapToMealPlanResponse(MealPlan mealPlan)
    {
        return new MealPlanResponse
        {
            Id = mealPlan.Id,
            WorkspaceId = mealPlan.WorkspaceId,
            CreatedByPtId = mealPlan.CreatedByPtId,
            Title = mealPlan.Title,
            ContentJson = mealPlan.ContentJson,
            Source = mealPlan.Source,
            IsActive = mealPlan.IsActive,
            CreatedAt = mealPlan.CreatedAt
        };
    }

    #endregion
}

