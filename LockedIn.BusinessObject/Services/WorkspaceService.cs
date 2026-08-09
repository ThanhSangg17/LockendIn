using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    private readonly IConfiguration _configuration;

    public WorkspaceService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IBookingService bookingService,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _bookingService = bookingService;
        _configuration = configuration;
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

    #endregion


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
            Status = s.Status,
            ScheduledStart = s.ScheduledStart,
            ScheduledEnd = s.ScheduledEnd,
            PtCheckedInAt = s.PtCheckedInAt,
            CustomerCheckedInAt = s.CustomerCheckedInAt,
            StartedAt = s.StartedAt,
            Description = s.Description,
            CompletedAt = s.CompletedAt
        }).ToList();

        var totalSessions = workspace.Booking.SessionCount;
        var completedSessionsCount = dbSessions.Count(s => s.Status == (int)WorkspaceSessionStatus.Completed);
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

    #region Proposal & Scheduling APIs

    public async Task<ApiResponse<SessionProposalResponse>> CreateProposalAsync(Guid workspaceId, CreateSessionProposalRequest request)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            return ApiResponse<SessionProposalResponse>.Fail("User is not authenticated.");

        if (_currentUserService.Role != (int)UserRole.PersonalTrainer)
            return ApiResponse<SessionProposalResponse>.Fail("Only personal trainers can create session proposals.");

        var userId = _currentUserService.UserId.Value;
        var ptProfile = await _unitOfWork.PtProfiles.Query()
            .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);

        if (ptProfile == null)
            return ApiResponse<SessionProposalResponse>.Fail("Personal trainer profile not found.");

        var workspace = await _unitOfWork.Workspaces.Query()
            .Include(w => w.Booking)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            return ApiResponse<SessionProposalResponse>.Fail("Workspace not found.");

        if (workspace.PtProfileId != ptProfile.Id)
            return ApiResponse<SessionProposalResponse>.Fail("You do not own this workspace.");

        if (workspace.Status != (int)WorkspaceStatus.Active || workspace.Booking.Status != (int)BookingStatus.Active)
            return ApiResponse<SessionProposalResponse>.Fail("Workspace or Booking is not active.");

        if (request.Slots == null || !request.Slots.Any())
            return ApiResponse<SessionProposalResponse>.Fail("At least one time slot must be proposed.");

        if (request.SessionNumber <= 0 || request.SessionNumber > workspace.Booking.SessionCount)
            return ApiResponse<SessionProposalResponse>.Fail($"Session number must be between 1 and {workspace.Booking.SessionCount}.");

        // Check if there is already a WorkspaceSession for this session number
        var existingSession = await _unitOfWork.WorkspaceSessions.Query()
            .FirstOrDefaultAsync(s => s.WorkspaceId == workspaceId && s.SessionNumber == request.SessionNumber);
        if (existingSession != null && existingSession.Status != (int)WorkspaceSessionStatus.Cancelled && existingSession.Status != (int)WorkspaceSessionStatus.Missed)
            return ApiResponse<SessionProposalResponse>.Fail($"Session number {request.SessionNumber} has already been scheduled or completed.");

        // Check if active proposal exists for this Workspace & SessionNumber
        var activeProposal = await _unitOfWork.SessionProposals.Query()
            .FirstOrDefaultAsync(p => p.WorkspaceId == workspaceId && p.SessionNumber == request.SessionNumber && p.Status == (int)SessionProposalStatus.Active);
        if (activeProposal != null)
            return ApiResponse<SessionProposalResponse>.Fail($"An active proposal for session {request.SessionNumber} already exists.");

        // Validate slots against FixedTimeSlotCatalog
        var proposalSlots = new List<SessionProposalSlot>();
        foreach (var slotDto in request.Slots)
        {
            var catalogSlot = FixedTimeSlotCatalog.GetByCode(slotDto.SlotCode);
            if (catalogSlot == null)
                return ApiResponse<SessionProposalResponse>.Fail($"Invalid slot code '{slotDto.SlotCode}'.");

            var slotDate = slotDto.Date.Date;
            var startDateTime = slotDate.Add(catalogSlot.StartTime);
            if (startDateTime < DateTime.UtcNow)
                return ApiResponse<SessionProposalResponse>.Fail($"Proposed slot on {slotDate:yyyy-MM-dd} {catalogSlot.DisplayName} is in the past.");

            proposalSlots.Add(new SessionProposalSlot
            {
                Id = Guid.NewGuid(),
                Date = slotDate,
                SlotCode = catalogSlot.SlotCode,
                IsSelected = false
            });
        }

        var proposal = new SessionProposal
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            SessionNumber = request.SessionNumber,
            Status = (int)SessionProposalStatus.Active,
            CreatedAt = DateTime.UtcNow,
            SessionProposalSlots = proposalSlots
        };

        await _unitOfWork.SessionProposals.AddAsync(proposal);

        // Send notification to Customer
        try
        {
            var customerUser = await _unitOfWork.CustomerProfiles.Query()
                .Where(c => c.Id == workspace.CustomerId)
                .Select(c => c.UserId)
                .FirstOrDefaultAsync();

            if (customerUser != Guid.Empty)
            {
                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = customerUser,
                    Title = "Đề xuất khung giờ tập mới",
                    Content = $"PT đã gửi các khung giờ đề xuất cho buổi tập #{request.SessionNumber}. Vui lòng vào workspace để chọn.",
                    Type = (int)NotificationType.Booking,
                    IsRead = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        catch
        {
            // Silently ignore notification failure
        }

        await _unitOfWork.SaveChangesAsync();

        return await GetProposalResponseInternalAsync(proposal);
    }

    public async Task<ApiResponse<SessionProposalResponse>> GetActiveProposalAsync(Guid workspaceId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            return ApiResponse<SessionProposalResponse>.Fail("User is not authenticated.");

        var workspace = await _unitOfWork.Workspaces.Query()
            .FirstOrDefaultAsync(w => w.Id == workspaceId);
        if (workspace == null)
            return ApiResponse<SessionProposalResponse>.Fail("Workspace not found.");

        var (allowed, error) = await CheckWorkspaceAccessAsync(workspace);
        if (!allowed)
            return ApiResponse<SessionProposalResponse>.Fail(error ?? "Access denied.");

        var proposal = await _unitOfWork.SessionProposals.Query()
            .Include(p => p.SessionProposalSlots)
            .Where(p => p.WorkspaceId == workspaceId && p.Status == (int)SessionProposalStatus.Active)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        if (proposal == null)
            return ApiResponse<SessionProposalResponse>.Fail("No active proposal found for this workspace.");

        return await GetProposalAvailabilityAsync(workspaceId, proposal.Id);
    }

    public async Task<ApiResponse<SessionProposalResponse>> GetProposalAvailabilityAsync(Guid workspaceId, Guid proposalId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            return ApiResponse<SessionProposalResponse>.Fail("User is not authenticated.");

        var workspace = await _unitOfWork.Workspaces.Query()
            .FirstOrDefaultAsync(w => w.Id == workspaceId);
        if (workspace == null)
            return ApiResponse<SessionProposalResponse>.Fail("Workspace not found.");

        var (allowed, error) = await CheckWorkspaceAccessAsync(workspace);
        if (!allowed)
            return ApiResponse<SessionProposalResponse>.Fail(error ?? "Access denied.");

        var proposal = await _unitOfWork.SessionProposals.Query()
            .Include(p => p.SessionProposalSlots)
            .FirstOrDefaultAsync(p => p.Id == proposalId && p.WorkspaceId == workspaceId);

        if (proposal == null)
            return ApiResponse<SessionProposalResponse>.Fail("Session proposal not found.");

        // Query all Scheduled & InProgress sessions for this PT across all workspaces
        var ptSessions = await _unitOfWork.WorkspaceSessions.Query()
            .Include(s => s.Workspace)
            .Where(s => s.Workspace.PtProfileId == workspace.PtProfileId &&
                        (s.Status == (int)WorkspaceSessionStatus.Scheduled || s.Status == (int)WorkspaceSessionStatus.InProgress))
            .ToListAsync();

        var slotResponses = new List<SessionProposalSlotResponse>();
        foreach (var slot in proposal.SessionProposalSlots)
        {
            var catalogSlot = FixedTimeSlotCatalog.GetByCode(slot.SlotCode);
            var startAt = VietnamTimeZoneHelper.CreateUtcFromVietnamDateAndTime(slot.Date, catalogSlot?.StartTime ?? TimeSpan.Zero);
            var endAt = VietnamTimeZoneHelper.CreateUtcFromVietnamDateAndTime(slot.Date, catalogSlot?.EndTime ?? TimeSpan.Zero);

            // Conflict condition: requestedStart < existingEnd AND requestedEnd > existingStart (All in UTC)
            var isConflict = ptSessions.Any(s =>
                s.ScheduledStart.HasValue && s.ScheduledEnd.HasValue &&
                startAt < s.ScheduledEnd.Value && endAt > s.ScheduledStart.Value);

            slotResponses.Add(new SessionProposalSlotResponse
            {
                Id = slot.Id,
                Date = slot.Date,
                SlotCode = slot.SlotCode,
                DisplayName = catalogSlot?.DisplayName ?? slot.SlotCode,
                StartTime = VietnamTimeZoneHelper.EnsureUtcKind(startAt),
                EndTime = VietnamTimeZoneHelper.EnsureUtcKind(endAt),
                IsSelected = slot.IsSelected,
                IsAvailable = !isConflict,
                Reason = isConflict ? "PT_ALREADY_BOOKED" : null
            });
        }


        var response = new SessionProposalResponse
        {
            Id = proposal.Id,
            WorkspaceId = proposal.WorkspaceId,
            SessionNumber = proposal.SessionNumber,
            Status = proposal.Status,
            CreatedAt = proposal.CreatedAt,
            Slots = slotResponses
        };

        return ApiResponse<SessionProposalResponse>.Ok(response, "Proposal availability retrieved successfully.");
    }

    public async Task<ApiResponse<bool>> RevokeProposalAsync(Guid workspaceId, Guid proposalId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            return ApiResponse<bool>.Fail("User is not authenticated.");

        if (_currentUserService.Role != (int)UserRole.PersonalTrainer)
            return ApiResponse<bool>.Fail("Only personal trainers can revoke proposals.");

        var userId = _currentUserService.UserId.Value;
        var ptProfile = await _unitOfWork.PtProfiles.Query()
            .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);

        if (ptProfile == null)
            return ApiResponse<bool>.Fail("Personal trainer profile not found.");

        var workspace = await _unitOfWork.Workspaces.Query().FirstOrDefaultAsync(w => w.Id == workspaceId);
        if (workspace == null || workspace.PtProfileId != ptProfile.Id)
            return ApiResponse<bool>.Fail("Workspace not found or access denied.");

        var proposal = await _unitOfWork.SessionProposals.Query()
            .FirstOrDefaultAsync(p => p.Id == proposalId && p.WorkspaceId == workspaceId);

        if (proposal == null)
            return ApiResponse<bool>.Fail("Proposal not found.");

        if (proposal.Status != (int)SessionProposalStatus.Active)
            return ApiResponse<bool>.Fail("Proposal is no longer active.");

        proposal.Status = (int)SessionProposalStatus.Revoked;
        proposal.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.SessionProposals.Update(proposal);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Proposal revoked successfully.");
    }

    public async Task<ApiResponse<WorkspaceSessionResponse>> SelectProposalSlotAsync(Guid workspaceId, Guid proposalId, SelectSlotRequest request)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            return ApiResponse<WorkspaceSessionResponse>.Fail("User is not authenticated.");

        if (_currentUserService.Role != (int)UserRole.Customer)
            return ApiResponse<WorkspaceSessionResponse>.Fail("Only customers can select a session slot.");

        var userId = _currentUserService.UserId.Value;
        var customerProfile = await _unitOfWork.CustomerProfiles.Query()
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);

        if (customerProfile == null)
            return ApiResponse<WorkspaceSessionResponse>.Fail("Customer profile not found.");

        var workspace = await _unitOfWork.Workspaces.Query()
            .Include(w => w.Booking)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            return ApiResponse<WorkspaceSessionResponse>.Fail("Workspace not found.");

        if (workspace.CustomerId != customerProfile.Id)
            return ApiResponse<WorkspaceSessionResponse>.Fail("You do not own this workspace.");

        if (workspace.Status != (int)WorkspaceStatus.Active || workspace.Booking.Status != (int)BookingStatus.Active)
            return ApiResponse<WorkspaceSessionResponse>.Fail("Workspace or Booking is not active.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var proposal = await _unitOfWork.SessionProposals.Query()
                .Include(p => p.SessionProposalSlots)
                .FirstOrDefaultAsync(p => p.Id == proposalId && p.WorkspaceId == workspaceId);

            if (proposal == null || proposal.Status != (int)SessionProposalStatus.Active)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<WorkspaceSessionResponse>.Fail("Proposal is no longer active or available.");
            }

            var slot = proposal.SessionProposalSlots.FirstOrDefault(s => s.Id == request.SlotId);
            if (slot == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<WorkspaceSessionResponse>.Fail("Selected slot does not belong to this proposal.");
            }

            var catalogSlot = FixedTimeSlotCatalog.GetByCode(slot.SlotCode);
            if (catalogSlot == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<WorkspaceSessionResponse>.Fail("Invalid slot code.");
            }

            var requestedStart = VietnamTimeZoneHelper.CreateUtcFromVietnamDateAndTime(slot.Date, catalogSlot?.StartTime ?? TimeSpan.Zero);
            var requestedEnd = VietnamTimeZoneHelper.CreateUtcFromVietnamDateAndTime(slot.Date, catalogSlot?.EndTime ?? TimeSpan.Zero);

            // Re-verify overlap conflict inside transaction across all PT sessions (in UTC)
            var overlapConflict = await _unitOfWork.WorkspaceSessions.Query()
                .Include(s => s.Workspace)
                .AnyAsync(s => s.Workspace.PtProfileId == workspace.PtProfileId &&
                               (s.Status == (int)WorkspaceSessionStatus.Scheduled || s.Status == (int)WorkspaceSessionStatus.InProgress) &&
                               s.ScheduledStart.HasValue && s.ScheduledEnd.HasValue &&
                               requestedStart < s.ScheduledEnd.Value && requestedEnd > s.ScheduledStart.Value);

            if (overlapConflict)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<WorkspaceSessionResponse>.Fail("HTTP 409 Conflict: PT has another session booked at this time. Please select another slot.");
            }

            // Check if session already scheduled or completed
            var existingSession = await _unitOfWork.WorkspaceSessions.Query()
                .FirstOrDefaultAsync(s => s.WorkspaceId == workspaceId && s.SessionNumber == proposal.SessionNumber);

            if (existingSession != null && existingSession.Status != (int)WorkspaceSessionStatus.Cancelled && existingSession.Status != (int)WorkspaceSessionStatus.Missed)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<WorkspaceSessionResponse>.Fail($"Session #{proposal.SessionNumber} has already been scheduled.");
            }

            // Mark slot selected & proposal accepted
            slot.IsSelected = true;
            proposal.Status = (int)SessionProposalStatus.Accepted;
            proposal.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.SessionProposals.Update(proposal);

            WorkspaceSession session;
            if (existingSession != null && (existingSession.Status == (int)WorkspaceSessionStatus.Missed || existingSession.Status == (int)WorkspaceSessionStatus.Cancelled))
            {
                // Reuse existing WorkspaceSession record to prevent duplicate SessionNumber
                session = existingSession;
                session.Status = (int)WorkspaceSessionStatus.Scheduled;
                session.ScheduledStart = requestedStart;
                session.ScheduledEnd = requestedEnd;
                session.PtCheckedInAt = null;
                session.CustomerCheckedInAt = null;
                session.StartedAt = null;
                session.CompletedAt = null;
                _unitOfWork.WorkspaceSessions.Update(session);
            }
            else
            {
                // Create Scheduled WorkspaceSession with UTC timestamps
                session = new WorkspaceSession
                {
                    Id = Guid.NewGuid(),
                    WorkspaceId = workspaceId,
                    SessionNumber = proposal.SessionNumber,
                    Status = (int)WorkspaceSessionStatus.Scheduled,
                    ScheduledStart = requestedStart,
                    ScheduledEnd = requestedEnd,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.WorkspaceSessions.AddAsync(session);
            }

            // Send Notification to PT
            try
            {
                var ptUser = await _unitOfWork.PtProfiles.Query()
                    .Where(pt => pt.Id == workspace.PtProfileId)
                    .Select(pt => pt.UserId)
                    .FirstOrDefaultAsync();

                if (ptUser != Guid.Empty)
                {
                    await _unitOfWork.Notifications.AddAsync(new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = ptUser,
                        Title = "Khách hàng đã chọn lịch tập",
                        Content = $"Khách hàng đã chọn khung giờ {catalogSlot?.DisplayName ?? slot.SlotCode} ngày {slot.Date:dd/MM/yyyy} cho Buổi #{proposal.SessionNumber}.",
                        Type = (int)NotificationType.Booking,
                        IsRead = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            catch
            {
                // ignore
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var response = new WorkspaceSessionResponse
            {
                Id = session.Id,
                WorkspaceId = session.WorkspaceId,
                SessionNumber = session.SessionNumber,
                Status = session.Status,
                ScheduledStart = VietnamTimeZoneHelper.EnsureUtcKind(session.ScheduledStart),
                ScheduledEnd = VietnamTimeZoneHelper.EnsureUtcKind(session.ScheduledEnd),
                PtCheckedInAt = VietnamTimeZoneHelper.EnsureUtcKind(session.PtCheckedInAt),
                CustomerCheckedInAt = VietnamTimeZoneHelper.EnsureUtcKind(session.CustomerCheckedInAt),
                StartedAt = VietnamTimeZoneHelper.EnsureUtcKind(session.StartedAt),
                Description = session.Description,
                CompletedAt = VietnamTimeZoneHelper.EnsureUtcKind(session.CompletedAt)
            };

            return ApiResponse<WorkspaceSessionResponse>.Ok(response, "Session slot selected and scheduled successfully.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<WorkspaceSessionResponse>.Fail($"Failed to select slot: {ex.Message}");
        }
    }

    private async Task<ApiResponse<SessionProposalResponse>> GetProposalResponseInternalAsync(SessionProposal proposal)
    {
        var slotResponses = proposal.SessionProposalSlots.Select(slot =>
        {
            var catalogSlot = FixedTimeSlotCatalog.GetByCode(slot.SlotCode);
            var startAt = slot.Date.Date.Add(catalogSlot?.StartTime ?? TimeSpan.Zero);
            var endAt = slot.Date.Date.Add(catalogSlot?.EndTime ?? TimeSpan.Zero);
            return new SessionProposalSlotResponse
            {
                Id = slot.Id,
                Date = slot.Date,
                SlotCode = slot.SlotCode,
                DisplayName = catalogSlot?.DisplayName ?? slot.SlotCode,
                StartTime = startAt,
                EndTime = endAt,
                IsSelected = slot.IsSelected,
                IsAvailable = true,
                Reason = null
            };
        }).ToList();

        var response = new SessionProposalResponse
        {
            Id = proposal.Id,
            WorkspaceId = proposal.WorkspaceId,
            SessionNumber = proposal.SessionNumber,
            Status = proposal.Status,
            CreatedAt = proposal.CreatedAt,
            Slots = slotResponses
        };

        return ApiResponse<SessionProposalResponse>.Ok(response, "Proposal created successfully.");
    }

    #endregion

    #region Check-in & Complete APIs


    public async Task<ApiResponse<WorkspaceSessionResponse>> CheckInSessionAsync(Guid workspaceId, Guid sessionId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            return ApiResponse<WorkspaceSessionResponse>.Fail("User is not authenticated.");

        var userId = _currentUserService.UserId.Value;
        var workspace = await _unitOfWork.Workspaces.Query().FirstOrDefaultAsync(w => w.Id == workspaceId);
        if (workspace == null)
            return ApiResponse<WorkspaceSessionResponse>.Fail("Workspace not found.");

        var session = await _unitOfWork.WorkspaceSessions.Query()
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.WorkspaceId == workspaceId);

        if (session == null)
            return ApiResponse<WorkspaceSessionResponse>.Fail("Workspace session not found.");

        if (session.Status != (int)WorkspaceSessionStatus.Scheduled && session.Status != (int)WorkspaceSessionStatus.InProgress)
            return ApiResponse<WorkspaceSessionResponse>.Fail("Session is not in Scheduled or InProgress state.");

        if (!session.ScheduledStart.HasValue)
            return ApiResponse<WorkspaceSessionResponse>.Fail("Session has no scheduled start time.");

        var now = DateTime.UtcNow;
        int leadMins = int.TryParse(_configuration["SessionScheduling:CheckInLeadTimeMinutes"], out var l) ? l : 15;
        int windowMins = int.TryParse(_configuration["SessionScheduling:CheckInWindowDurationMinutes"], out var w) ? w : 45;

        var openTime = session.ScheduledStart.Value.AddMinutes(-leadMins);
        var closeTime = session.ScheduledStart.Value.AddMinutes(windowMins - leadMins); // e.g. -15m + 45m = +30m

        if (now < openTime)
            return ApiResponse<WorkspaceSessionResponse>.Fail($"Check-in opens {leadMins} minutes before scheduled start time.");

        if (now > closeTime)
            return ApiResponse<WorkspaceSessionResponse>.Fail("Check-in window has closed for this session.");

        bool isPt = false;
        bool isCust = false;

        if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await _unitOfWork.PtProfiles.Query().FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);
            if (ptProfile != null && workspace.PtProfileId == ptProfile.Id)
                isPt = true;
        }
        else if (_currentUserService.Role == (int)UserRole.Customer)
        {
            var customerProfile = await _unitOfWork.CustomerProfiles.Query().FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
            if (customerProfile != null && workspace.CustomerId == customerProfile.Id)
                isCust = true;
        }

        if (!isPt && !isCust)
            return ApiResponse<WorkspaceSessionResponse>.Fail("Access denied to check-in for this session.");

        if (isPt)
        {
            session.PtCheckedInAt = now;
        }
        if (isCust)
        {
            session.CustomerCheckedInAt = now;
        }

        // Dual Check-in transition to InProgress
        if (session.PtCheckedInAt.HasValue && session.CustomerCheckedInAt.HasValue)
        {
            session.Status = (int)WorkspaceSessionStatus.InProgress;
            if (!session.StartedAt.HasValue)
                session.StartedAt = now;
        }

        _unitOfWork.WorkspaceSessions.Update(session);

        // Send notification to counter-party
        try
        {
            Guid recipientUserId = Guid.Empty;
            string actorName = isPt ? "PT" : "Khách hàng";
            if (isPt)
            {
                recipientUserId = await _unitOfWork.CustomerProfiles.Query()
                    .Where(c => c.Id == workspace.CustomerId).Select(c => c.UserId).FirstOrDefaultAsync();
            }
            else
            {
                recipientUserId = await _unitOfWork.PtProfiles.Query()
                    .Where(pt => pt.Id == workspace.PtProfileId).Select(pt => pt.UserId).FirstOrDefaultAsync();
            }

            if (recipientUserId != Guid.Empty)
            {
                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = recipientUserId,
                    Title = "Điểm danh buổi tập",
                    Content = $"{actorName} đã điểm danh cho Buổi #{session.SessionNumber}.",
                    Type = (int)NotificationType.Booking,
                    IsRead = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        catch
        {
            // ignore
        }

        await _unitOfWork.SaveChangesAsync();

        var response = new WorkspaceSessionResponse
        {
            Id = session.Id,
            WorkspaceId = session.WorkspaceId,
            SessionNumber = session.SessionNumber,
            Status = session.Status,
            ScheduledStart = VietnamTimeZoneHelper.EnsureUtcKind(session.ScheduledStart),
            ScheduledEnd = VietnamTimeZoneHelper.EnsureUtcKind(session.ScheduledEnd),
            PtCheckedInAt = VietnamTimeZoneHelper.EnsureUtcKind(session.PtCheckedInAt),
            CustomerCheckedInAt = VietnamTimeZoneHelper.EnsureUtcKind(session.CustomerCheckedInAt),
            StartedAt = VietnamTimeZoneHelper.EnsureUtcKind(session.StartedAt),
            Description = session.Description,
            CompletedAt = VietnamTimeZoneHelper.EnsureUtcKind(session.CompletedAt)
        };

        return ApiResponse<WorkspaceSessionResponse>.Ok(response, "Check-in successful.");

    }

    public async Task<ApiResponse<WorkspaceSessionResponse>> CompleteSessionAsync(Guid workspaceId, Guid sessionId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            return ApiResponse<WorkspaceSessionResponse>.Fail("User is not authenticated.");

        if (_currentUserService.Role != (int)UserRole.PersonalTrainer)
            return ApiResponse<WorkspaceSessionResponse>.Fail("Only personal trainers can complete a session.");

        var userId = _currentUserService.UserId.Value;
        var ptProfile = await _unitOfWork.PtProfiles.Query().FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);
        if (ptProfile == null)
            return ApiResponse<WorkspaceSessionResponse>.Fail("Personal trainer profile not found.");

        var workspace = await _unitOfWork.Workspaces.Query()
            .Include(w => w.Booking)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null || workspace.PtProfileId != ptProfile.Id)
            return ApiResponse<WorkspaceSessionResponse>.Fail("Workspace not found or access denied.");

        var session = await _unitOfWork.WorkspaceSessions.Query()
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.WorkspaceId == workspaceId);

        if (session == null)
            return ApiResponse<WorkspaceSessionResponse>.Fail("Workspace session not found.");

        if (session.Status != (int)WorkspaceSessionStatus.InProgress)
            return ApiResponse<WorkspaceSessionResponse>.Fail("Session must be InProgress before it can be completed.");

        if (!session.PtCheckedInAt.HasValue || !session.CustomerCheckedInAt.HasValue)
            return ApiResponse<WorkspaceSessionResponse>.Fail("Both PT and Customer must check-in before completing the session.");

        await _unitOfWork.BeginTransactionAsync();
        BookingCompletionResult? completionResult = null;
        try
        {
            session.Status = (int)WorkspaceSessionStatus.Completed;
            session.CompletedAt = DateTime.UtcNow;
            _unitOfWork.WorkspaceSessions.Update(session);

            var completedCount = await _unitOfWork.WorkspaceSessions.Query()
                .CountAsync(s => s.WorkspaceId == workspaceId && (s.Status == (int)WorkspaceSessionStatus.Completed || s.Id == sessionId));

            if (completedCount >= workspace.Booking.SessionCount)
            {
                workspace.Status = (int)WorkspaceStatus.Closed;
                workspace.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Workspaces.Update(workspace);

                completionResult = await _bookingService.CompleteBookingCoreAsync(workspace.Booking);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<WorkspaceSessionResponse>.Fail($"Failed to complete session: {ex.Message}");
        }

        var response = new WorkspaceSessionResponse
        {
            Id = session.Id,
            WorkspaceId = session.WorkspaceId,
            SessionNumber = session.SessionNumber,
            Status = session.Status,
            ScheduledStart = VietnamTimeZoneHelper.EnsureUtcKind(session.ScheduledStart),
            ScheduledEnd = VietnamTimeZoneHelper.EnsureUtcKind(session.ScheduledEnd),
            PtCheckedInAt = VietnamTimeZoneHelper.EnsureUtcKind(session.PtCheckedInAt),
            CustomerCheckedInAt = VietnamTimeZoneHelper.EnsureUtcKind(session.CustomerCheckedInAt),
            StartedAt = VietnamTimeZoneHelper.EnsureUtcKind(session.StartedAt),
            Description = session.Description,
            CompletedAt = VietnamTimeZoneHelper.EnsureUtcKind(session.CompletedAt)
        };

        return ApiResponse<WorkspaceSessionResponse>.Ok(response, "Workspace session completed successfully.");

    }

    #endregion
}




