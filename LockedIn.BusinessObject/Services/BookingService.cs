using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.DTOs.Bookings;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public BookingService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<BookingResponse>> CreateBookingAsync(CreateBookingRequest request)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<BookingResponse>.Fail("User is not authenticated.");
        }

        if (_currentUserService.Role != (int)UserRole.Customer)
        {
            return ApiResponse<BookingResponse>.Fail("Only customers can create bookings.");
        }

        var userId = _currentUserService.UserId.Value;
        var customerProfile = await _unitOfWork.CustomerProfiles.Query()
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);

        if (customerProfile == null)
        {
            return ApiResponse<BookingResponse>.Fail("Customer profile not found.");
        }

        var package = await _unitOfWork.Packages.Query()
            .Include(p => p.PtProfile)
            .FirstOrDefaultAsync(p => p.Id == request.PackageId && !p.IsDeleted && p.IsActive);

        if (package == null)
        {
            return ApiResponse<BookingResponse>.Fail("Package not found or inactive.");
        }

        var ptProfile = package.PtProfile;
        if (ptProfile == null || ptProfile.IsDeleted || ptProfile.VerificationStatus != (int)PtVerificationStatus.Approved)
        {
            return ApiResponse<BookingResponse>.Fail("Personal trainer profile is not approved or deleted.");
        }

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            CustomerId = customerProfile.Id,
            PtProfileId = package.PtProfileId,
            PackageId = package.Id,
            Status = (int)BookingStatus.PendingTrainerAcceptance,
            TotalAmount = package.Price,
            SessionCount = package.SessionCount,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Bookings.AddAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToBookingResponse(booking);
        return ApiResponse<BookingResponse>.Ok(response, "Booking created successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<BookingResponse>>> GetMyBookingsAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<IReadOnlyList<BookingResponse>>.Fail("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;
        IQueryable<Booking> query = _unitOfWork.Bookings.Query();

        if (_currentUserService.Role == (int)UserRole.Customer)
        {
            var customerProfile = await _unitOfWork.CustomerProfiles.Query()
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
            if (customerProfile == null)
            {
                return ApiResponse<IReadOnlyList<BookingResponse>>.Fail("Customer profile not found.");
            }
            query = query.Where(b => b.CustomerId == customerProfile.Id);
        }
        else if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await _unitOfWork.PtProfiles.Query()
                .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);
            if (ptProfile == null)
            {
                return ApiResponse<IReadOnlyList<BookingResponse>>.Fail("Personal trainer profile not found.");
            }
            query = query.Where(b => b.PtProfileId == ptProfile.Id);
        }
        else if (_currentUserService.Role == (int)UserRole.Admin)
        {
            // Admin can view all bookings, so no filter is applied
        }
        else
        {
            return ApiResponse<IReadOnlyList<BookingResponse>>.Fail("Access denied or invalid user role.");
        }

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var response = bookings.Select(MapToBookingResponse).ToList();
        return ApiResponse<IReadOnlyList<BookingResponse>>.Ok(response, "Bookings retrieved successfully.");
    }

    public async Task<ApiResponse<BookingDetailResponse>> GetBookingByIdAsync(Guid bookingId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<BookingDetailResponse>.Fail("User is not authenticated.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            return ApiResponse<BookingDetailResponse>.Fail("Booking not found");
        }

        var userId = _currentUserService.UserId.Value;

        if (_currentUserService.Role == (int)UserRole.Customer)
        {
            var customerProfile = await _unitOfWork.CustomerProfiles.Query()
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
            if (customerProfile == null || booking.CustomerId != customerProfile.Id)
            {
                return ApiResponse<BookingDetailResponse>.Fail("Access denied to this booking.");
            }
        }
        else if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await _unitOfWork.PtProfiles.Query()
                .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);
            if (ptProfile == null || booking.PtProfileId != ptProfile.Id)
            {
                return ApiResponse<BookingDetailResponse>.Fail("Access denied to this booking.");
            }
        }
        else if (_currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<BookingDetailResponse>.Fail("Access denied to this booking.");
        }

        var response = MapToBookingDetailResponse(booking);
        return ApiResponse<BookingDetailResponse>.Ok(response, "Booking details retrieved successfully.");
    }

    public async Task<ApiResponse<BookingResponse>> CancelBookingAsync(Guid bookingId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<BookingResponse>.Fail("User is not authenticated.");
        }

        if (_currentUserService.Role != (int)UserRole.Customer)
        {
            return ApiResponse<BookingResponse>.Fail("Only customers can cancel bookings.");
        }

        var userId = _currentUserService.UserId.Value;
        var customerProfile = await _unitOfWork.CustomerProfiles.Query()
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);

        if (customerProfile == null)
        {
            return ApiResponse<BookingResponse>.Fail("Customer profile not found.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            return ApiResponse<BookingResponse>.Fail("Booking not found");
        }

        if (booking.CustomerId != customerProfile.Id)
        {
            return ApiResponse<BookingResponse>.Fail("You do not own this booking.");
        }

        if (booking.Status != (int)BookingStatus.PendingPayment && booking.Status != (int)BookingStatus.PendingTrainerAcceptance)
        {
            return ApiResponse<BookingResponse>.Fail("Booking can only be cancelled if it is pending payment or pending acceptance.");
        }

        booking.Status = (int)BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToBookingResponse(booking);
        return ApiResponse<BookingResponse>.Ok(response, "Booking cancelled successfully.");
    }

    public async Task<ApiResponse<BookingResponse>> AcceptBookingAsync(Guid bookingId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<BookingResponse>.Fail("User is not authenticated.");
        }

        if (_currentUserService.Role != (int)UserRole.PersonalTrainer)
        {
            return ApiResponse<BookingResponse>.Fail("Only personal trainers can accept bookings.");
        }

        var userId = _currentUserService.UserId.Value;
        var ptProfile = await _unitOfWork.PtProfiles.Query()
            .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);

        if (ptProfile == null)
        {
            return ApiResponse<BookingResponse>.Fail("Personal trainer profile not found.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            return ApiResponse<BookingResponse>.Fail("Booking not found");
        }

        if (booking.PtProfileId != ptProfile.Id)
        {
            return ApiResponse<BookingResponse>.Fail("You are not the assigned personal trainer for this booking.");
        }

        if (booking.Status != (int)BookingStatus.PendingTrainerAcceptance)
        {
            return ApiResponse<BookingResponse>.Fail("Booking can only be accepted if it is pending trainer acceptance.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            booking.Status = (int)BookingStatus.PendingPayment;
            booking.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Bookings.Update(booking);

            try
            {
                var customerProfile = await _unitOfWork.CustomerProfiles.Query()
                    .FirstOrDefaultAsync(c => c.Id == booking.CustomerId);
                if (customerProfile != null)
                {
                    var notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = customerProfile.UserId,
                        Title = "Booking accepted",
                        Content = "Your personal trainer accepted the booking. Please complete the payment to activate your package.",
                        Type = (int)NotificationType.Booking,
                        IsRead = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Notifications.AddAsync(notification);
                }
            }
            catch
            {
                // silently ignore
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<BookingResponse>.Fail($"Failed to accept booking: {ex.Message}");
        }

        // Audit log after commit (best-effort)
        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = ptProfile.UserId,
                Action = "AcceptBooking",
                EntityName = "Booking",
                EntityId = booking.Id,
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    bookingId = booking.Id,
                    ptProfileId = booking.PtProfileId,
                    customerId = booking.CustomerId
                }),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            // silently ignore
        }

        var response = MapToBookingResponse(booking);
        return ApiResponse<BookingResponse>.Ok(response, "Booking accepted successfully.");
    }

    public async Task<ApiResponse<BookingResponse>> RejectBookingAsync(Guid bookingId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<BookingResponse>.Fail("User is not authenticated.");
        }

        if (_currentUserService.Role != (int)UserRole.PersonalTrainer)
        {
            return ApiResponse<BookingResponse>.Fail("Only personal trainers can reject bookings.");
        }

        var userId = _currentUserService.UserId.Value;
        var ptProfile = await _unitOfWork.PtProfiles.Query()
            .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);

        if (ptProfile == null)
        {
            return ApiResponse<BookingResponse>.Fail("Personal trainer profile not found.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            return ApiResponse<BookingResponse>.Fail("Booking not found");
        }

        if (booking.PtProfileId != ptProfile.Id)
        {
            return ApiResponse<BookingResponse>.Fail("You are not the assigned personal trainer for this booking.");
        }

        if (booking.Status != (int)BookingStatus.PendingTrainerAcceptance)
        {
            return ApiResponse<BookingResponse>.Fail("Booking can only be rejected if it is pending trainer acceptance.");
        }

        booking.Status = (int)BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToBookingResponse(booking);
        return ApiResponse<BookingResponse>.Ok(response, "Booking rejected successfully.");
    }

    public async Task<ApiResponse<BookingResponse>> CompleteBookingAsync(Guid bookingId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<BookingResponse>.Fail("User is not authenticated.");
        }

        if (_currentUserService.Role != (int)UserRole.PersonalTrainer)
        {
            return ApiResponse<BookingResponse>.Fail("Only personal trainers can complete bookings.");
        }

        var userId = _currentUserService.UserId.Value;
        var ptProfile = await _unitOfWork.PtProfiles.Query()
            .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);

        if (ptProfile == null)
        {
            return ApiResponse<BookingResponse>.Fail("Personal trainer profile not found.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .Include(b => b.Workspace)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            return ApiResponse<BookingResponse>.Fail("Booking not found");
        }

        if (booking.PtProfileId != ptProfile.Id)
        {
            return ApiResponse<BookingResponse>.Fail("You are not the assigned personal trainer for this booking.");
        }

        if (booking.Status != (int)BookingStatus.Active)
        {
            return ApiResponse<BookingResponse>.Fail("Booking can only be completed if it is active.");
        }

        if (booking.Workspace == null)
        {
            return ApiResponse<BookingResponse>.Fail("Workspace not found for this booking.");
        }

        // Kiểm tra số lượng session thực tế đã hoàn thành
        var completedSessionsCount = await _unitOfWork.WorkspaceSessions.Query()
            .CountAsync(s => s.WorkspaceId == booking.Workspace.Id);

        if (completedSessionsCount < booking.SessionCount)
        {
            return ApiResponse<BookingResponse>.Fail($"Cannot complete booking: only {completedSessionsCount}/{booking.SessionCount} sessions are completed.");
        }

        await _unitOfWork.BeginTransactionAsync();
        BookingCompletionResult completionResult;
        try
        {
            // Close Workspace if active
            if (booking.Workspace.Status != (int)WorkspaceStatus.Closed)
            {
                booking.Workspace.Status = (int)WorkspaceStatus.Closed;
                booking.Workspace.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Workspaces.Update(booking.Workspace);
            }

            completionResult = await CompleteBookingCoreAsync(booking);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<BookingResponse>.Fail($"Failed to complete booking: {ex.Message}");
        }

        // Audit log after commit (best-effort)
        try
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
                    workspaceId = booking.Workspace.Id,
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

            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            // silently ignore
        }

        var response = MapToBookingResponse(booking);
        return ApiResponse<BookingResponse>.Ok(response, "Booking completed successfully.");
    }

    public async Task<BookingCompletionResult> CompleteBookingCoreAsync(Booking booking)
    {
        // 1. Update Booking status
        booking.Status = (int)BookingStatus.CompletedPendingSettlement;
        booking.CompletedAt = DateTime.UtcNow;
        booking.SettlementDueAt = DateTime.UtcNow.AddHours(48);
        booking.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Bookings.Update(booking);

        // 2. Create Settlement if not exists
        var existingSettlement = await _unitOfWork.Settlements.Query()
            .FirstOrDefaultAsync(s => s.BookingId == booking.Id);

        Guid settlementId = Guid.NewGuid();
        if (existingSettlement == null)
        {
            var platformFee = booking.TotalAmount * 0.10m;
            var netAmount = booking.TotalAmount - platformFee;

            var settlement = new Settlement
            {
                Id = settlementId,
                BookingId = booking.Id,
                PtProfileId = booking.PtProfileId,
                GrossAmount = booking.TotalAmount,
                PlatformFee = platformFee,
                NetAmount = netAmount,
                Status = (int)SettlementStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var openDispute = await _unitOfWork.Disputes.Query()
                .AnyAsync(d => d.BookingId == booking.Id && (d.Status == (int)DisputeStatus.Open || d.Status == (int)DisputeStatus.UnderReview));

            if (openDispute)
            {
                settlement.Status = (int)SettlementStatus.BlockedByDispute;
            }

            await _unitOfWork.Settlements.AddAsync(settlement);
        }
        else
        {
            settlementId = existingSettlement.Id;
        }

        // 3. Create Notification for Customer if not exists
        Guid customerUserId = Guid.Empty;
        var customerProfile = await _unitOfWork.CustomerProfiles.Query()
            .FirstOrDefaultAsync(c => c.Id == booking.CustomerId);
        if (customerProfile != null)
        {
            customerUserId = customerProfile.UserId;
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = customerProfile.UserId,
                Title = "Booking completed",
                Content = "Your booking has been completed. You can now leave a review.",
                Type = (int)NotificationType.Booking,
                IsRead = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Notifications.AddAsync(notification);
        }

        Guid ptUserId = Guid.Empty;
        var ptProfile = await _unitOfWork.PtProfiles.Query()
            .FirstOrDefaultAsync(p => p.Id == booking.PtProfileId);
        if (ptProfile != null)
        {
            ptUserId = ptProfile.UserId;
        }

        return new BookingCompletionResult(
            booking.Id,
            customerUserId,
            ptUserId,
            booking.TotalAmount,
            settlementId
        );
    }

    #region Helper Methods

    private BookingResponse MapToBookingResponse(Booking booking)
    {
        return new BookingResponse
        {
            Id = booking.Id,
            CustomerId = booking.CustomerId,
            PtProfileId = booking.PtProfileId,
            PackageId = booking.PackageId,
            Status = booking.Status,
            TotalAmount = booking.TotalAmount,
            SessionCount = booking.SessionCount,
            CreatedAt = booking.CreatedAt
        };
    }

    private BookingDetailResponse MapToBookingDetailResponse(Booking booking)
    {
        return new BookingDetailResponse
        {
            Id = booking.Id,
            CustomerId = booking.CustomerId,
            PtProfileId = booking.PtProfileId,
            PackageId = booking.PackageId,
            Status = booking.Status,
            TotalAmount = booking.TotalAmount,
            SessionCount = booking.SessionCount,
            PaidAt = booking.PaidAt,
            StartedAt = booking.StartedAt,
            CompletedAt = booking.CompletedAt,
            SettlementDueAt = booking.SettlementDueAt,
            CreatedAt = booking.CreatedAt
        };
    }

    #endregion
}

