using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.DTOs.Workspaces;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Services;

public class WorkspaceService : IWorkspaceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBookingService _bookingService;

    public WorkspaceService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IBookingService bookingService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _bookingService = bookingService;
    }

    public async Task<ApiResponse<WorkspaceResponse>> GetWorkspaceByBookingAsync(Guid bookingId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<WorkspaceResponse>.Fail("User is not authenticated.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            return ApiResponse<WorkspaceResponse>.Fail("Booking not found.");
        }

        var (allowed, error) = await CheckBookingAccessAsync(booking);
        if (!allowed)
        {
            return ApiResponse<WorkspaceResponse>.Fail(error ?? "Access denied.");
        }

        var workspace = await _unitOfWork.Workspaces.Query()
            .FirstOrDefaultAsync(w => w.BookingId == bookingId);

        if (workspace == null)
        {
            return ApiResponse<WorkspaceResponse>.Fail("Workspace not found.");
        }

        var response = MapToWorkspaceResponse(workspace);
        return ApiResponse<WorkspaceResponse>.Ok(response, "Workspace retrieved successfully.");
    }

    public async Task<ApiResponse<WorkspaceResponse>> GetWorkspaceByIdAsync(Guid workspaceId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<WorkspaceResponse>.Fail("User is not authenticated.");
        }

        var workspace = await _unitOfWork.Workspaces.Query()
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
        {
            return ApiResponse<WorkspaceResponse>.Fail("Workspace not found.");
        }

        var (allowed, error) = await CheckWorkspaceAccessAsync(workspace);
        if (!allowed)
        {
            return ApiResponse<WorkspaceResponse>.Fail(error ?? "Access denied.");
        }

        var response = MapToWorkspaceResponse(workspace);
        return ApiResponse<WorkspaceResponse>.Ok(response, "Workspace retrieved successfully.");
    }

    public async Task<ApiResponse<WorkspaceResponse>> UpdateCourseNoteAsync(Guid workspaceId, UpdateCourseNoteRequest request)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<WorkspaceResponse>.Fail("User is not authenticated.");
        }

        var workspace = await _unitOfWork.Workspaces.Query()
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
        {
            return ApiResponse<WorkspaceResponse>.Fail("Workspace not found.");
        }

        var userId = _currentUserService.UserId.Value;

        if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await _unitOfWork.PtProfiles.Query()
                .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);
            if (ptProfile == null || workspace.PtProfileId != ptProfile.Id)
            {
                return ApiResponse<WorkspaceResponse>.Fail("You are not the personal trainer assigned to this workspace.");
            }
        }
        else if (_currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<WorkspaceResponse>.Fail("Only personal trainers or admins can update the course note.");
        }

        workspace.CourseNote = request.CourseNote;
        workspace.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Workspaces.Update(workspace);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToWorkspaceResponse(workspace);
        return ApiResponse<WorkspaceResponse>.Ok(response, "Course note updated successfully.");
    }

    #region Access Control Helpers & Mapping

    private async Task<(bool Allowed, string? Error)> CheckWorkspaceAccessAsync(Workspace workspace)
    {
        var userId = _currentUserService.UserId!.Value;

        if (_currentUserService.Role == (int)UserRole.Customer)
        {
            var customerProfile = await _unitOfWork.CustomerProfiles.Query()
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
            if (customerProfile == null || workspace.CustomerId != customerProfile.Id)
            {
                return (false, "Access denied to this workspace.");
            }
        }
        else if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await _unitOfWork.PtProfiles.Query()
                .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);
            if (ptProfile == null || workspace.PtProfileId != ptProfile.Id)
            {
                return (false, "Access denied to this workspace.");
            }
        }
        else if (_currentUserService.Role != (int)UserRole.Admin)
        {
            return (false, "Access denied to this workspace.");
        }

        return (true, null);
    }

    private async Task<(bool Allowed, string? Error)> CheckBookingAccessAsync(Booking booking)
    {
        var userId = _currentUserService.UserId!.Value;

        if (_currentUserService.Role == (int)UserRole.Customer)
        {
            var customerProfile = await _unitOfWork.CustomerProfiles.Query()
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
            if (customerProfile == null || booking.CustomerId != customerProfile.Id)
            {
                return (false, "Access denied to this booking.");
            }
        }
        else if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await _unitOfWork.PtProfiles.Query()
                .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);
            if (ptProfile == null || booking.PtProfileId != ptProfile.Id)
            {
                return (false, "Access denied to this booking.");
            }
        }
        else if (_currentUserService.Role != (int)UserRole.Admin)
        {
            return (false, "Access denied to this booking.");
        }

        return (true, null);
    }

    private WorkspaceResponse MapToWorkspaceResponse(Workspace workspace)
    {
        return new WorkspaceResponse
        {
            Id = workspace.Id,
            BookingId = workspace.BookingId,
            CustomerId = workspace.CustomerId,
            PtProfileId = workspace.PtProfileId,
            Status = workspace.Status,
            CourseNote = workspace.CourseNote,
            CreatedAt = workspace.CreatedAt
        };
    }

    public async Task<ApiResponse<WorkspaceSessionResponse>> CreateSessionAsync(Guid workspaceId, CreateWorkspaceSessionRequest request)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<WorkspaceSessionResponse>.Fail("User is not authenticated.");
        }

        if (_currentUserService.Role != (int)UserRole.PersonalTrainer)
        {
            return ApiResponse<WorkspaceSessionResponse>.Fail("Only personal trainers can create sessions.");
        }

        var userId = _currentUserService.UserId.Value;
        var ptProfile = await _unitOfWork.PtProfiles.Query()
            .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);

        if (ptProfile == null)
        {
            return ApiResponse<WorkspaceSessionResponse>.Fail("Personal trainer profile not found.");
        }

        var workspace = await _unitOfWork.Workspaces.Query()
            .Include(w => w.Booking)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
        {
            return ApiResponse<WorkspaceSessionResponse>.Fail("Workspace not found.");
        }

        if (workspace.PtProfileId != ptProfile.Id)
        {
            return ApiResponse<WorkspaceSessionResponse>.Fail("You do not own this workspace.");
        }

        if (workspace.Status != (int)WorkspaceStatus.Active)
        {
            return ApiResponse<WorkspaceSessionResponse>.Fail("Workspace is not active.");
        }

        var booking = workspace.Booking;
        if (booking == null)
        {
            return ApiResponse<WorkspaceSessionResponse>.Fail("Associated booking not found.");
        }

        if (booking.Status != (int)BookingStatus.Active)
        {
            return ApiResponse<WorkspaceSessionResponse>.Fail("Associated booking is not active.");
        }

        await _unitOfWork.BeginTransactionAsync();
        WorkspaceSession session;
        BookingCompletionResult? completionResult = null;
        int nextSessionNumber = 0;

        try
        {
            var currentSessionsCount = await _unitOfWork.WorkspaceSessions.Query()
                .CountAsync(s => s.WorkspaceId == workspaceId);

            if (currentSessionsCount >= booking.SessionCount)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<WorkspaceSessionResponse>.Fail("Cannot create more sessions than booking session count.");
            }

            nextSessionNumber = currentSessionsCount + 1;

            session = new WorkspaceSession
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspaceId,
                SessionNumber = nextSessionNumber,
                Description = request.Description,
                CompletedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.WorkspaceSessions.AddAsync(session);

            if (nextSessionNumber == booking.SessionCount)
            {
                workspace.Status = (int)WorkspaceStatus.Closed;
                workspace.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Workspaces.Update(workspace);

                completionResult = await _bookingService.CompleteBookingCoreAsync(booking);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (DbUpdateException dbEx) when (dbEx.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true || dbEx.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<WorkspaceSessionResponse>.Fail("Conflict: A session with this number has already been recorded.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<WorkspaceSessionResponse>.Fail($"Failed to create workspace session: {ex.Message}");
        }

        // Audit log after commit (best-effort)
        try
        {
            var sessionAudit = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = ptProfile.UserId,
                Action = "CreateWorkspaceSession",
                EntityName = "WorkspaceSession",
                EntityId = session.Id,
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    bookingId = booking.Id,
                    workspaceId = workspace.Id,
                    sessionNumber = nextSessionNumber,
                    totalSessions = booking.SessionCount
                }),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(sessionAudit);

            if (completionResult != null)
            {
                var bookingAudit = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    ActorUserId = ptProfile.UserId,
                    Action = "CompleteBooking",
                    EntityName = "Booking",
                    EntityId = completionResult.BookingId,
                    MetadataJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        bookingId = completionResult.BookingId,
                        workspaceId = workspace.Id,
                        totalSessions = booking.SessionCount
                    }),
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.AuditLogs.AddAsync(bookingAudit);

                var settlementAudit = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    ActorUserId = ptProfile.UserId,
                    Action = "CreateSettlement",
                    EntityName = "Settlement",
                    EntityId = completionResult.SettlementId,
                    MetadataJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        bookingId = completionResult.BookingId,
                        settlementId = completionResult.SettlementId,
                        grossAmount = completionResult.TotalAmount
                    }),
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.AuditLogs.AddAsync(settlementAudit);
            }

            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            // silently ignore
        }

        var response = new WorkspaceSessionResponse
        {
            Id = session.Id,
            WorkspaceId = session.WorkspaceId,
            SessionNumber = session.SessionNumber,
            Description = session.Description,
            CompletedAt = session.CompletedAt
        };

        return ApiResponse<WorkspaceSessionResponse>.Ok(response, "Workspace session created successfully.");
    }

    public async Task<ApiResponse<WorkspaceProgressResponse>> GetWorkspaceProgressAsync(Guid workspaceId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<WorkspaceProgressResponse>.Fail("User is not authenticated.");
        }

        var workspace = await _unitOfWork.Workspaces.Query()
            .Include(w => w.Booking)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
        {
            return ApiResponse<WorkspaceProgressResponse>.Fail("Workspace not found.");
        }

        var (allowed, error) = await CheckWorkspaceAccessAsync(workspace);
        if (!allowed)
        {
            return ApiResponse<WorkspaceProgressResponse>.Fail(error ?? "Access denied.");
        }

        var dbSessions = await _unitOfWork.WorkspaceSessions.Query()
            .Where(s => s.WorkspaceId == workspaceId)
            .OrderBy(s => s.SessionNumber)
            .ToListAsync();

        var sessionResponses = dbSessions.Select(s => new WorkspaceSessionResponse
        {
            Id = s.Id,
            WorkspaceId = s.WorkspaceId,
            SessionNumber = s.SessionNumber,
            Description = s.Description,
            CompletedAt = s.CompletedAt
        }).ToList();

        var totalSessions = workspace.Booking.SessionCount;
        var completedSessionsCount = sessionResponses.Count;
        var remainingSessions = Math.Max(0, totalSessions - completedSessionsCount);

        var response = new WorkspaceProgressResponse
        {
            TotalSessions = totalSessions,
            CompletedSessionsCount = completedSessionsCount,
            RemainingSessions = remainingSessions,
            Sessions = sessionResponses
        };

        return ApiResponse<WorkspaceProgressResponse>.Ok(response, "Workspace progress retrieved successfully.");
    }

    #endregion
}

