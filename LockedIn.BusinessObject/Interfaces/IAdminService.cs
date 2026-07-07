using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Admin;
using LockedIn.BusinessObject.DTOs.Disputes;
using LockedIn.BusinessObject.DTOs.PtProfiles;
using LockedIn.BusinessObject.DTOs.PtProfile;

namespace LockedIn.BusinessObject.Interfaces;

public interface IAdminService
{
    Task<ApiResponse<DashboardResponse>> GetDashboardAsync();
    Task<ApiResponse<IReadOnlyList<AdminUserResponse>>> GetUsersAsync();
    Task<ApiResponse<AdminUserResponse>> GetUserByIdAsync(Guid userId);
    Task<ApiResponse<AdminUserResponse>> BanUserAsync(Guid userId);
    Task<ApiResponse<AdminUserResponse>> UnbanUserAsync(Guid userId);
    Task<ApiResponse<IReadOnlyList<PtProfileResponse>>> GetPtVerificationsAsync();
    Task<ApiResponse<PtVerificationDetailResponse>> GetPtVerificationByIdAsync(Guid ptProfileId);
    Task<ApiResponse<PtProfileResponse>> ApprovePtAsync(Guid ptProfileId);
    Task<ApiResponse<PtProfileResponse>> RejectPtAsync(Guid ptProfileId, RejectPtRequest request);
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
    Task<ApiResponse<IReadOnlyList<ProfileEditRequestResponse>>> GetPtProfileEditRequestsAsync(int? status = null);
    Task<ApiResponse<ProfileEditRequestResponse>> GetPtProfileEditRequestByIdAsync(Guid requestId);
    Task<ApiResponse<ProfileEditRequestResponse>> ApprovePtProfileEditRequestAsync(Guid requestId);
    Task<ApiResponse<ProfileEditRequestResponse>> RejectPtProfileEditRequestAsync(Guid requestId, RejectProfileEditRequest request);

    Task<ApiResponse<AddonProductResponse>> CreateAddonProductAsync(CreateAddonProductRequest request);
    Task<ApiResponse<PagedResult<AddonProductResponse>>> GetAddonProductsAsync(PaginationRequest request, string? search = null, int? productType = null, bool? isActive = null);
    Task<ApiResponse<AddonProductResponse>> GetAddonProductByIdAsync(Guid productId);
    Task<ApiResponse<AddonProductResponse>> ActivateAddonProductAsync(Guid productId);
    Task<ApiResponse<AddonProductResponse>> DeactivateAddonProductAsync(Guid productId);
    Task<ApiResponse<AddonProductPriceResponse>> CreateAddonProductPriceAsync(Guid productId, CreateAddonProductPriceRequest request);
    Task<ApiResponse<IReadOnlyList<AddonProductPriceResponse>>> GetAddonProductPricesAsync(Guid productId);
}
