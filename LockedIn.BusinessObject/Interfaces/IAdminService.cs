using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Admin;
using LockedIn.BusinessObject.DTOs.Disputes;
using LockedIn.BusinessObject.DTOs.PtProfiles;

namespace LockedIn.BusinessObject.Interfaces;

public interface IAdminService
{
    Task<ApiResponse<DashboardResponse>> GetDashboardAsync();
    Task<ApiResponse<IReadOnlyList<AdminUserResponse>>> GetUsersAsync();
    Task<ApiResponse<AdminUserResponse>> GetUserByIdAsync(Guid userId);
    Task<ApiResponse<AdminUserResponse>> BanUserAsync(Guid userId);
    Task<ApiResponse<AdminUserResponse>> UnbanUserAsync(Guid userId);
    Task<ApiResponse<IReadOnlyList<PtProfileResponse>>> GetPtVerificationsAsync();
    Task<ApiResponse<PtProfileResponse>> ApprovePtAsync(Guid ptProfileId);
    Task<ApiResponse<PtProfileResponse>> RejectPtAsync(Guid ptProfileId);
    Task<ApiResponse<IReadOnlyList<AdminPaymentResponse>>> GetPaymentsAsync();
    Task<ApiResponse<AdminPaymentResponse>> GetPaymentByIdAsync(Guid paymentId);
    Task<ApiResponse<IReadOnlyList<AdminDisputeResponse>>> GetDisputesAsync();
    Task<ApiResponse<AdminDisputeResponse>> MarkDisputeUnderReviewAsync(Guid disputeId);
    Task<ApiResponse<AdminDisputeResponse>> ResolveRefundCustomerAsync(Guid disputeId, ResolveDisputeRequest request);
    Task<ApiResponse<AdminDisputeResponse>> ResolveReleaseToPtAsync(Guid disputeId, ResolveDisputeRequest request);
    Task<ApiResponse<IReadOnlyList<AdminSettlementResponse>>> GetSettlementsAsync();
    Task<ApiResponse<AdminSettlementResponse>> ApproveSettlementAsync(Guid settlementId);
    Task<ApiResponse<AdminSettlementResponse>> MarkSettlementAsSettledAsync(Guid settlementId);
    Task<ApiResponse<IReadOnlyList<AuditLogResponse>>> GetAuditLogsAsync();
}
