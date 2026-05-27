using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _service;

    public PaymentsController(IPaymentService service)
    {
        _service = service;
    }

    [HttpPost("create-link")]
    public async Task<IActionResult> CreatePaymentLinkAsync()
    {
        var result = await _service.CreatePaymentLinkAsync();
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
    public async Task<IActionResult> HandlePayOsWebhookAsync()
    {
        var result = await _service.HandlePayOsWebhookAsync();
        return Ok(result);
    }

}
