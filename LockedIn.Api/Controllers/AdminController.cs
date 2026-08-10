using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.DTOs.Disputes;
using LockedIn.BusinessObject.DTOs.Admin;
using LockedIn.BusinessObject.DTOs.Transactions;
using LockedIn.BusinessObject.Common;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "3,Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _service;
    private readonly ITransactionService _transactionService;

    public AdminController(IAdminService service, ITransactionService transactionService)
    {
        _service = service;
        _transactionService = transactionService;
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetAdminTransactionsAsync([FromQuery] TransactionSearchRequest request)
    {
        var result = await _transactionService.GetAdminTransactionsAsync(request);
        if (!result.Success)
        {
            if (result.Message != null && result.Message.Contains("Only Admins"))
                return StatusCode(403, result);
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("dashboard/analytics")]
    public async Task<IActionResult> GetDashboardAnalyticsAsync()
    {
        var result = await _service.GetDashboardAnalyticsAsync();
        return Ok(result);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardAsync()
    {
        var result = await _service.GetDashboardAsync();
        return Ok(result);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsersAsync()
    {
        var result = await _service.GetUsersAsync();
        return Ok(result);
    }

    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserByIdAsync(Guid userId)
    {
        var result = await _service.GetUserByIdAsync(userId);
        return Ok(result);
    }

    [HttpPatch("users/{userId}/ban")]
    public async Task<IActionResult> BanUserAsync(Guid userId)
    {
        var result = await _service.BanUserAsync(userId);
        return Ok(result);
    }

    [HttpPatch("users/{userId}/unban")]
    public async Task<IActionResult> UnbanUserAsync(Guid userId)
    {
        var result = await _service.UnbanUserAsync(userId);
        return Ok(result);
    }

    [HttpGet("pt-verifications")]
    public async Task<IActionResult> GetPtVerificationsAsync()
    {
        var result = await _service.GetPtVerificationsAsync();
        return Ok(result);
    }

    [HttpGet("pt-verifications/{ptProfileId}")]
    public async Task<IActionResult> GetPtVerificationByIdAsync(Guid ptProfileId)
    {
        var result = await _service.GetPtVerificationByIdAsync(ptProfileId);
        return Ok(result);
    }

    [HttpPost("pt-verifications/{ptProfileId}/approve")]
    public async Task<IActionResult> ApprovePtAsync(Guid ptProfileId)
    {
        var result = await _service.ApprovePtAsync(ptProfileId);
        return Ok(result);
    }

    [HttpPost("pt-verifications/{ptProfileId}/reject")]
    public async Task<IActionResult> RejectPtAsync(Guid ptProfileId, [FromBody] LockedIn.BusinessObject.DTOs.Admin.RejectPtRequest request)
    {
        var result = await _service.RejectPtAsync(ptProfileId, request);
        return Ok(result);
    }

    [HttpGet("payments")]
    public async Task<IActionResult> GetPaymentsAsync()
    {
        var result = await _service.GetPaymentsAsync();
        return Ok(result);
    }

    [HttpGet("payments/{paymentId}")]
    public async Task<IActionResult> GetPaymentByIdAsync(Guid paymentId)
    {
        var result = await _service.GetPaymentByIdAsync(paymentId);
        return Ok(result);
    }

    [HttpGet("disputes")]
    public async Task<IActionResult> GetDisputesAsync()
    {
        var result = await _service.GetDisputesAsync();
        return Ok(result);
    }

    [HttpGet("disputes/{disputeId}")]
    public async Task<IActionResult> GetDisputeByIdAsync(Guid disputeId)
    {
        var result = await _service.GetDisputeByIdAsync(disputeId);
        return Ok(result);
    }

    [HttpPost("disputes/{disputeId}/under-review")]
    public async Task<IActionResult> MarkDisputeUnderReviewAsync(Guid disputeId)
    {
        var result = await _service.MarkDisputeUnderReviewAsync(disputeId);
        return Ok(result);
    }

    [HttpPost("disputes/{disputeId}/resolve-refund-customer")]
    public async Task<IActionResult> ResolveRefundCustomerAsync(Guid disputeId, [FromBody] ResolveDisputeRequest request)
    {
        var result = await _service.ResolveRefundCustomerAsync(disputeId, request);
        return Ok(result);
    }

    [HttpPost("disputes/{disputeId}/resolve-release-to-pt")]
    public async Task<IActionResult> ResolveReleaseToPtAsync(Guid disputeId, [FromBody] ResolveDisputeRequest request)
    {
        var result = await _service.ResolveReleaseToPtAsync(disputeId, request);
        return Ok(result);
    }

    [HttpGet("settlements")]
    public async Task<IActionResult> GetSettlementsAsync()
    {
        var result = await _service.GetSettlementsAsync();
        return Ok(result);
    }

    [HttpPost("settlements/{settlementId}/approve")]
    public async Task<IActionResult> ApproveSettlementAsync(Guid settlementId)
    {
        var result = await _service.ApproveSettlementAsync(settlementId);
        return Ok(result);
    }

    [HttpPost("settlements/{settlementId}/mark-settled")]
    public async Task<IActionResult> MarkSettlementAsSettledAsync(Guid settlementId)
    {
        var result = await _service.MarkSettlementAsSettledAsync(settlementId);
        return Ok(result);
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogsAsync()
    {
        var result = await _service.GetAuditLogsAsync();
        return Ok(result);
    }

    [HttpGet("pt-profile-edit-requests")]
    public async Task<IActionResult> GetPtProfileEditRequestsAsync([FromQuery] int? status = null)
    {
        var result = await _service.GetPtProfileEditRequestsAsync(status);
        return Ok(result);
    }

    [HttpGet("pt-profile-edit-requests/{requestId}")]
    public async Task<IActionResult> GetPtProfileEditRequestByIdAsync(Guid requestId)
    {
        var result = await _service.GetPtProfileEditRequestByIdAsync(requestId);
        return Ok(result);
    }

    [HttpPost("pt-profile-edit-requests/{requestId}/approve")]
    public async Task<IActionResult> ApprovePtProfileEditRequestAsync(Guid requestId)
    {
        var result = await _service.ApprovePtProfileEditRequestAsync(requestId);
        return Ok(result);
    }

    [HttpPost("pt-profile-edit-requests/{requestId}/reject")]
    public async Task<IActionResult> RejectPtProfileEditRequestAsync(Guid requestId, [FromBody] LockedIn.BusinessObject.DTOs.Admin.RejectProfileEditRequest request)
    {
        var result = await _service.RejectPtProfileEditRequestAsync(requestId, request);
        return Ok(result);
    }

    [HttpPost("addon-products")]
    public async Task<IActionResult> CreateAddonProductAsync([FromBody] CreateAddonProductRequest request)
    {
        var result = await _service.CreateAddonProductAsync(request);
        return Ok(result);
    }

    [HttpGet("addon-products")]
    public async Task<IActionResult> GetAddonProductsAsync(
        [FromQuery] PaginationRequest request,
        [FromQuery] string? search = null,
        [FromQuery] int? productType = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _service.GetAddonProductsAsync(request, search, productType, isActive);
        return Ok(result);
    }

    [HttpGet("addon-products/{productId}")]
    public async Task<IActionResult> GetAddonProductByIdAsync(Guid productId)
    {
        var result = await _service.GetAddonProductByIdAsync(productId);
        return Ok(result);
    }

    [HttpPatch("addon-products/{productId}/activate")]
    public async Task<IActionResult> ActivateAddonProductAsync(Guid productId)
    {
        var result = await _service.ActivateAddonProductAsync(productId);
        return Ok(result);
    }

    [HttpPatch("addon-products/{productId}/deactivate")]
    public async Task<IActionResult> DeactivateAddonProductAsync(Guid productId)
    {
        var result = await _service.DeactivateAddonProductAsync(productId);
        return Ok(result);
    }

    [HttpPost("addon-products/{productId}/prices")]
    public async Task<IActionResult> CreateAddonProductPriceAsync(Guid productId, [FromBody] CreateAddonProductPriceRequest request)
    {
        var result = await _service.CreateAddonProductPriceAsync(productId, request);
        return Ok(result);
    }

    [HttpGet("addon-products/{productId}/prices")]
    public async Task<IActionResult> GetAddonProductPricesAsync(Guid productId)
    {
        var result = await _service.GetAddonProductPricesAsync(productId);
        return Ok(result);
    }
}
