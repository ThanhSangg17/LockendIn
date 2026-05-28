using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.DTOs.Payments;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _service;

    public PaymentsController(IPaymentService service)
    {
        _service = service;
    }

    [HttpPost("create-link")]
    public async Task<IActionResult> CreatePaymentLinkAsync([FromBody] CreatePaymentLinkRequest request)
    {
        var result = await _service.CreatePaymentLinkAsync(request);
        return Ok(result);
    }

    [HttpGet("booking/{bookingId}")]
    public async Task<IActionResult> GetPaymentByBookingAsync(Guid bookingId)
    {
        var result = await _service.GetPaymentByBookingAsync(bookingId);
        return Ok(result);
    }

    [HttpGet("{paymentId}")]
    public async Task<IActionResult> GetPaymentByIdAsync(Guid paymentId)
    {
        var result = await _service.GetPaymentByIdAsync(paymentId);
        return Ok(result);
    }

    [HttpPost("payos/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandlePayOsWebhookAsync([FromBody] PayOsWebhookRequest request)
    {
        var result = await _service.HandlePayOsWebhookAsync(request);
        return Ok(result);
    }
}
