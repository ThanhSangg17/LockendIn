using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.DTOs.Disputes;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IAdminService _service;

    public AdminController(IAdminService service)
    {
        _service = service;
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

    [HttpPost("pt-verifications/{ptProfileId}/approve")]
    public async Task<IActionResult> ApprovePtAsync(Guid ptProfileId)
    {
        var result = await _service.ApprovePtAsync(ptProfileId);
        return Ok(result);
    }

    [HttpPost("pt-verifications/{ptProfileId}/reject")]
    public async Task<IActionResult> RejectPtAsync(Guid ptProfileId)
    {
        var result = await _service.RejectPtAsync(ptProfileId);
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
}
