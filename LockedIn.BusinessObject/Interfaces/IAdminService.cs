using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.Interfaces;

public interface IAdminService
{
    Task<ApiResponse<string>> GetDashboardAsync();
    Task<ApiResponse<string>> GetUsersAsync();
    Task<ApiResponse<string>> GetUserByIdAsync(Guid userId);
    Task<ApiResponse<string>> BanUserAsync(Guid userId);
    Task<ApiResponse<string>> UnbanUserAsync(Guid userId);
    Task<ApiResponse<string>> GetPtVerificationsAsync();
    Task<ApiResponse<string>> ApprovePtAsync(Guid ptProfileId);
    Task<ApiResponse<string>> RejectPtAsync(Guid ptProfileId);
    Task<ApiResponse<string>> GetPaymentsAsync();
    Task<ApiResponse<string>> GetPaymentByIdAsync(Guid paymentId);
    Task<ApiResponse<string>> GetDisputesAsync();
    Task<ApiResponse<string>> MarkDisputeUnderReviewAsync(Guid disputeId);
    Task<ApiResponse<string>> ResolveRefundCustomerAsync(Guid disputeId);
    Task<ApiResponse<string>> ResolveReleaseToPtAsync(Guid disputeId);
    Task<ApiResponse<string>> GetSettlementsAsync();
    Task<ApiResponse<string>> ApproveSettlementAsync(Guid settlementId);
    Task<ApiResponse<string>> MarkSettlementAsSettledAsync(Guid settlementId);
    Task<ApiResponse<string>> GetAuditLogsAsync();
}
