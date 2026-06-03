using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.DTOs.MealPlans;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Services;

public class MealPlanService : IMealPlanService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public MealPlanService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<MealPlanResponse>> GenerateMealPlanAsync(GenerateMealPlanRequest request)
    {
        var pt = await GetCurrentPtProfileAsync();
        if (pt == null)
        {
            return ApiResponse<MealPlanResponse>.Fail("Only personal trainers can generate AI meal plans.");
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

        var mockContentObj = new
        {
            goal = request.Goal,
            preference = request.Preference,
            allergyNote = request.AllergyNote,
            days = new[]
            {
                new
                {
                    day = "Monday",
                    breakfast = "Oatmeal with banana",
                    lunch = "Chicken breast with brown rice",
                    dinner = "Salmon with vegetables"
                }
            }
        };
        var contentJson = System.Text.Json.JsonSerializer.Serialize(mockContentObj);

        var mealPlan = new MealPlan
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            CreatedByPtId = pt.Id,
            Title = "AI Meal Plan",
            ContentJson = contentJson,
            Source = 2, // AI = 2
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var aiLog = new AiUsageLog
        {
            Id = Guid.NewGuid(),
            PtProfileId = pt.Id,
            Feature = "AI_MEAL_PLAN",
            TokenUsed = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.MealPlans.AddAsync(mealPlan);
        await _unitOfWork.AiUsageLogs.AddAsync(aiLog);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToMealPlanResponse(mealPlan);
        return ApiResponse<MealPlanResponse>.Ok(response, "AI meal plan generated successfully.");
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

