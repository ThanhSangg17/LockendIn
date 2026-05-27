using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.DTOs.Admin;
using LockedIn.BusinessObject.DTOs.Disputes;
using LockedIn.BusinessObject.DTOs.PtProfiles;

namespace LockedIn.BusinessObject.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
        return await Task.FromResult(ApiResponse<IReadOnlyList<AdminDisputeResponse>>.Ok(new List<AdminDisputeResponse>(), "Not implemented yet"));
    }

    public async Task<ApiResponse<AdminDisputeResponse>> MarkDisputeUnderReviewAsync(Guid disputeId)
    {
        return await Task.FromResult(ApiResponse<AdminDisputeResponse>.Ok(new AdminDisputeResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<AdminDisputeResponse>> ResolveRefundCustomerAsync(Guid disputeId, ResolveDisputeRequest request)
    {
        return await Task.FromResult(ApiResponse<AdminDisputeResponse>.Ok(new AdminDisputeResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<AdminDisputeResponse>> ResolveReleaseToPtAsync(Guid disputeId, ResolveDisputeRequest request)
    {
        return await Task.FromResult(ApiResponse<AdminDisputeResponse>.Ok(new AdminDisputeResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<IReadOnlyList<AdminSettlementResponse>>> GetSettlementsAsync()
    {
        return await Task.FromResult(ApiResponse<IReadOnlyList<AdminSettlementResponse>>.Ok(new List<AdminSettlementResponse>(), "Not implemented yet"));
    }

    public async Task<ApiResponse<AdminSettlementResponse>> ApproveSettlementAsync(Guid settlementId)
    {
        return await Task.FromResult(ApiResponse<AdminSettlementResponse>.Ok(new AdminSettlementResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<AdminSettlementResponse>> MarkSettlementAsSettledAsync(Guid settlementId)
    {
        return await Task.FromResult(ApiResponse<AdminSettlementResponse>.Ok(new AdminSettlementResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<IReadOnlyList<AuditLogResponse>>> GetAuditLogsAsync()
    {
        return await Task.FromResult(ApiResponse<IReadOnlyList<AuditLogResponse>>.Ok(new List<AuditLogResponse>(), "Not implemented yet"));
    }
}
