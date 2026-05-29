using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.DTOs.Admin;
using LockedIn.BusinessObject.DTOs.Disputes;
using LockedIn.BusinessObject.DTOs.PtProfiles;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AdminService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<DashboardResponse>> GetDashboardAsync()
    {
        return await Task.FromResult(ApiResponse<DashboardResponse>.Ok(new DashboardResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<IReadOnlyList<AdminUserResponse>>> GetUsersAsync()
    {
        return await Task.FromResult(ApiResponse<IReadOnlyList<AdminUserResponse>>.Ok(new List<AdminUserResponse>(), "Not implemented yet"));
    }

    public async Task<ApiResponse<AdminUserResponse>> GetUserByIdAsync(Guid userId)
    {
        return await Task.FromResult(ApiResponse<AdminUserResponse>.Ok(new AdminUserResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<AdminUserResponse>> BanUserAsync(Guid userId)
    {
        return await Task.FromResult(ApiResponse<AdminUserResponse>.Ok(new AdminUserResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<AdminUserResponse>> UnbanUserAsync(Guid userId)
    {
        return await Task.FromResult(ApiResponse<AdminUserResponse>.Ok(new AdminUserResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<IReadOnlyList<PtProfileResponse>>> GetPtVerificationsAsync()
    {
        return await Task.FromResult(ApiResponse<IReadOnlyList<PtProfileResponse>>.Ok(new List<PtProfileResponse>(), "Not implemented yet"));
    }

    public async Task<ApiResponse<PtProfileResponse>> ApprovePtAsync(Guid ptProfileId)
    {
        return await Task.FromResult(ApiResponse<PtProfileResponse>.Ok(new PtProfileResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<PtProfileResponse>> RejectPtAsync(Guid ptProfileId)
    {
        return await Task.FromResult(ApiResponse<PtProfileResponse>.Ok(new PtProfileResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<IReadOnlyList<AdminPaymentResponse>>> GetPaymentsAsync()
    {
        return await Task.FromResult(ApiResponse<IReadOnlyList<AdminPaymentResponse>>.Ok(new List<AdminPaymentResponse>(), "Not implemented yet"));
    }

    public async Task<ApiResponse<AdminPaymentResponse>> GetPaymentByIdAsync(Guid paymentId)
    {
        return await Task.FromResult(ApiResponse<AdminPaymentResponse>.Ok(new AdminPaymentResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<IReadOnlyList<AdminDisputeResponse>>> GetDisputesAsync()
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<IReadOnlyList<AdminDisputeResponse>>.Fail("Only Admins can perform this action.");
        }

        var disputes = await _unitOfWork.Disputes.Query()
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var response = disputes.Select(d => new AdminDisputeResponse
        {
            Id = d.Id,
            BookingId = d.BookingId,
            CustomerId = d.CustomerId,
            PtProfileId = d.PtProfileId,
            Reason = d.Reason,
            Status = d.Status,
            CreatedAt = d.CreatedAt
        }).ToList();

        return ApiResponse<IReadOnlyList<AdminDisputeResponse>>.Ok(response, "Disputes retrieved successfully.");
    }

    public async Task<ApiResponse<AdminDisputeResponse>> MarkDisputeUnderReviewAsync(Guid disputeId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Only Admins can perform this action.");
        }

        var dispute = await _unitOfWork.Disputes.Query()
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute not found.");
        }

        if (dispute.Status != (int)DisputeStatus.Open)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute must be Open to be marked as Under Review.");
        }

        dispute.Status = (int)DisputeStatus.UnderReview;
        dispute.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Disputes.Update(dispute);
        await _unitOfWork.SaveChangesAsync();

        var response = new AdminDisputeResponse
        {
            Id = dispute.Id,
            BookingId = dispute.BookingId,
            CustomerId = dispute.CustomerId,
            PtProfileId = dispute.PtProfileId,
            Reason = dispute.Reason,
            Status = dispute.Status,
            CreatedAt = dispute.CreatedAt
        };

        return ApiResponse<AdminDisputeResponse>.Ok(response, "Dispute marked under review successfully.");
    }

    public async Task<ApiResponse<AdminDisputeResponse>> ResolveRefundCustomerAsync(Guid disputeId, ResolveDisputeRequest request)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Only Admins can perform this action.");
        }

        var dispute = await _unitOfWork.Disputes.Query()
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute not found.");
        }

        if (dispute.Status != (int)DisputeStatus.Open && dispute.Status != (int)DisputeStatus.UnderReview)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute is already resolved.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .FirstOrDefaultAsync(b => b.Id == dispute.BookingId);

        if (booking == null)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Associated booking not found.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            dispute.Status = (int)DisputeStatus.ResolvedRefundCustomer;
            dispute.ResolutionNote = !string.IsNullOrWhiteSpace(request?.ResolutionNote) ? request.ResolutionNote : "Refund customer";
            dispute.ResolvedByAdminId = _currentUserService.UserId!.Value;
            dispute.ResolvedAt = DateTime.UtcNow;
            dispute.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Disputes.Update(dispute);

            booking.Status = (int)BookingStatus.Refunded;
            booking.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Bookings.Update(booking);

            var payment = await _unitOfWork.Payments.Query()
                .FirstOrDefaultAsync(p => p.BookingId == booking.Id && p.Status == (int)PaymentStatus.Success);

            if (payment != null)
            {
                payment.Status = (int)PaymentStatus.Refunded;
                _unitOfWork.Payments.Update(payment);
            }

            var settlement = await _unitOfWork.Settlements.Query()
                .FirstOrDefaultAsync(s => s.BookingId == booking.Id);

            if (settlement != null)
            {
                settlement.Status = (int)SettlementStatus.BlockedByDispute;
                settlement.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Settlements.Update(settlement);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<AdminDisputeResponse>.Fail($"Failed to resolve dispute: {ex.Message}");
        }

        var response = new AdminDisputeResponse
        {
            Id = dispute.Id,
            BookingId = dispute.BookingId,
            CustomerId = dispute.CustomerId,
            PtProfileId = dispute.PtProfileId,
            Reason = dispute.Reason,
            Status = dispute.Status,
            CreatedAt = dispute.CreatedAt
        };

        return ApiResponse<AdminDisputeResponse>.Ok(response, "Dispute resolved and customer refunded successfully.");
    }

    public async Task<ApiResponse<AdminDisputeResponse>> ResolveReleaseToPtAsync(Guid disputeId, ResolveDisputeRequest request)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Only Admins can perform this action.");
        }

        var dispute = await _unitOfWork.Disputes.Query()
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute not found.");
        }

        if (dispute.Status != (int)DisputeStatus.Open && dispute.Status != (int)DisputeStatus.UnderReview)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute is already resolved.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .FirstOrDefaultAsync(b => b.Id == dispute.BookingId);

        if (booking == null)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Associated booking not found.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            dispute.Status = (int)DisputeStatus.ResolvedReleaseToPT;
            dispute.ResolutionNote = !string.IsNullOrWhiteSpace(request?.ResolutionNote) ? request.ResolutionNote : "Release to PT";
            dispute.ResolvedByAdminId = _currentUserService.UserId!.Value;
            dispute.ResolvedAt = DateTime.UtcNow;
            dispute.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Disputes.Update(dispute);

            booking.Status = (int)BookingStatus.CompletedPendingSettlement;
            booking.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Bookings.Update(booking);

            var settlement = await _unitOfWork.Settlements.Query()
                .FirstOrDefaultAsync(s => s.BookingId == booking.Id);

            if (settlement != null)
            {
                settlement.Status = (int)SettlementStatus.Pending;
                settlement.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Settlements.Update(settlement);
            }
            else
            {
                var platformFee = booking.TotalAmount * 0.10m;
                var netAmount = booking.TotalAmount - platformFee;

                settlement = new Settlement
                {
                    Id = Guid.NewGuid(),
                    BookingId = booking.Id,
                    PtProfileId = booking.PtProfileId,
                    GrossAmount = booking.TotalAmount,
                    PlatformFee = platformFee,
                    NetAmount = netAmount,
                    Status = (int)SettlementStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Settlements.AddAsync(settlement);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<AdminDisputeResponse>.Fail($"Failed to resolve dispute: {ex.Message}");
        }

        var response = new AdminDisputeResponse
        {
            Id = dispute.Id,
            BookingId = dispute.BookingId,
            CustomerId = dispute.CustomerId,
            PtProfileId = dispute.PtProfileId,
            Reason = dispute.Reason,
            Status = dispute.Status,
            CreatedAt = dispute.CreatedAt
        };

        return ApiResponse<AdminDisputeResponse>.Ok(response, "Dispute resolved and funds released to PT successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<AdminSettlementResponse>>> GetSettlementsAsync()
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<IReadOnlyList<AdminSettlementResponse>>.Fail("Only Admins can perform this action.");
        }

        var settlements = await _unitOfWork.Settlements.Query()
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var response = settlements.Select(s => new AdminSettlementResponse
        {
            Id = s.Id,
            BookingId = s.BookingId,
            PtProfileId = s.PtProfileId,
            GrossAmount = s.GrossAmount,
            PlatformFee = s.PlatformFee,
            NetAmount = s.NetAmount,
            Status = s.Status,
            CreatedAt = s.CreatedAt
        }).ToList();

        return ApiResponse<IReadOnlyList<AdminSettlementResponse>>.Ok(response, "Settlements retrieved successfully.");
    }

    public async Task<ApiResponse<AdminSettlementResponse>> ApproveSettlementAsync(Guid settlementId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AdminSettlementResponse>.Fail("Only Admins can perform this action.");
        }

        var settlement = await _unitOfWork.Settlements.Query()
            .FirstOrDefaultAsync(s => s.Id == settlementId);

        if (settlement == null)
        {
            return ApiResponse<AdminSettlementResponse>.Fail("Settlement not found.");
        }

        if (settlement.Status != (int)SettlementStatus.Pending)
        {
            return ApiResponse<AdminSettlementResponse>.Fail("Settlement must be Pending to be approved.");
        }

        var hasOpenDispute = await _unitOfWork.Disputes.Query()
            .AnyAsync(d => d.BookingId == settlement.BookingId && 
                           (d.Status == (int)DisputeStatus.Open || d.Status == (int)DisputeStatus.UnderReview));

        if (hasOpenDispute)
        {
            settlement.Status = (int)SettlementStatus.BlockedByDispute;
            settlement.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Settlements.Update(settlement);
            await _unitOfWork.SaveChangesAsync();

            var blockedResponse = MapToAdminSettlementResponse(settlement);
            return ApiResponse<AdminSettlementResponse>.Fail("Settlement blocked by dispute.");
        }

        settlement.Status = (int)SettlementStatus.Approved;
        settlement.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Settlements.Update(settlement);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToAdminSettlementResponse(settlement);
        return ApiResponse<AdminSettlementResponse>.Ok(response, "Settlement approved successfully.");
    }

    public async Task<ApiResponse<AdminSettlementResponse>> MarkSettlementAsSettledAsync(Guid settlementId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AdminSettlementResponse>.Fail("Only Admins can perform this action.");
        }

        var settlement = await _unitOfWork.Settlements.Query()
            .FirstOrDefaultAsync(s => s.Id == settlementId);

        if (settlement == null)
        {
            return ApiResponse<AdminSettlementResponse>.Fail("Settlement not found.");
        }

        if (settlement.Status != (int)SettlementStatus.Approved)
        {
            return ApiResponse<AdminSettlementResponse>.Fail("Settlement must be Approved before it can be settled.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .FirstOrDefaultAsync(b => b.Id == settlement.BookingId);

        if (booking == null)
        {
            return ApiResponse<AdminSettlementResponse>.Fail("Associated booking not found.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            settlement.Status = (int)SettlementStatus.Settled;
            settlement.SettledAt = DateTime.UtcNow;
            settlement.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Settlements.Update(settlement);

            booking.Status = (int)BookingStatus.Settled;
            booking.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Bookings.Update(booking);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<AdminSettlementResponse>.Fail($"Failed to settle: {ex.Message}");
        }

        var response = MapToAdminSettlementResponse(settlement);
        return ApiResponse<AdminSettlementResponse>.Ok(response, "Settlement marked as settled successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<AuditLogResponse>>> GetAuditLogsAsync()
    {
        return await Task.FromResult(ApiResponse<IReadOnlyList<AuditLogResponse>>.Ok(new List<AuditLogResponse>(), "Not implemented yet"));
    }

    #region Helper Methods

    private AdminSettlementResponse MapToAdminSettlementResponse(Settlement settlement)
    {
        return new AdminSettlementResponse
        {
            Id = settlement.Id,
            BookingId = settlement.BookingId,
            PtProfileId = settlement.PtProfileId,
            GrossAmount = settlement.GrossAmount,
            PlatformFee = settlement.PlatformFee,
            NetAmount = settlement.NetAmount,
            Status = settlement.Status,
            CreatedAt = settlement.CreatedAt
        };
    }

    #endregion
}
