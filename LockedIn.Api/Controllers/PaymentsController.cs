using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.DTOs.Payments;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _service;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService service, ILogger<PaymentsController> logger)
    {
        _service = service;
        _logger = logger;
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

    [HttpPost("{paymentId}/cancel")]
    public async Task<IActionResult> CancelPaymentAsync(Guid paymentId)
    {
        var result = await _service.CancelPaymentAsync(paymentId);
        if (!result.Success)
        {
            if (result.Message == "Payment not found.")
            {
                return NotFound(result);
            }
            if (result.Message == "You do not own this payment/booking." || 
                result.Message == "Only customers and admins can cancel payment links.")
            {
                return StatusCode(403, result);
            }
            if (result.Message == "User is not authenticated.")
            {
                return Unauthorized(result);
            }
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("payos/return")]
    [AllowAnonymous]
    public IActionResult PayOsReturn()
    {
        // ReturnUrl is only for user redirect testing. Real payment confirmation must be handled later by PayOS webhook or payment status verification.
        var queryParameters = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
        
        _logger.LogInformation("PayOS Return Callback received query parameters: {Parameters}", 
            System.Text.Json.JsonSerializer.Serialize(queryParameters));

        return Ok(new
        {
            Message = "Redirected from PayOS successfully (Return).",
            QueryParameters = queryParameters
        });
    }

    [HttpGet("payos/cancel")]
    [AllowAnonymous]
    public IActionResult PayOsCancel()
    {
        // ReturnUrl is only for user redirect testing. Real payment confirmation must be handled later by PayOS webhook or payment status verification.
        var queryParameters = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());

        _logger.LogInformation("PayOS Cancel Callback received query parameters: {Parameters}", 
            System.Text.Json.JsonSerializer.Serialize(queryParameters));

        return Ok(new
        {
            Message = "Redirected from PayOS successfully (Cancel).",
            QueryParameters = queryParameters
        });
    }

    [HttpGet("payos/webhook")]
    [AllowAnonymous]
    public IActionResult GetPayOsWebhookHealth()
    {
        return Ok(new
        {
            success = true,
            message = "PayOS webhook endpoint is reachable. Use POST for real webhooks."
        });
    }

    [HttpPost("payos/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandlePayOsWebhookAsync()
    {
        // 1. Enable buffering and read raw body safely
        Request.EnableBuffering();
        
        string rawBody = "";
        try
        {
            using (var reader = new System.IO.StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                rawBody = await reader.ReadToEndAsync();
                Request.Body.Position = 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read request body.");
        }

        // 2. Log request details (excluding secrets/Authorization/Cookies)
        var headers = Request.Headers
            .Where(h => !h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) && 
                        !h.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(h => h.Key, h => h.Value.ToString());

        _logger.LogInformation(
            "PayOS Webhook Request received:\nMethod: {Method}\nPath: {Path}\nContentType: {ContentType}\nQueryString: {QueryString}\nHeaders: {Headers}\nBody: {Body}",
            Request.Method,
            Request.Path,
            Request.ContentType,
            Request.QueryString.ToString(),
            System.Text.Json.JsonSerializer.Serialize(headers),
            rawBody
        );

        // 3. Check for empty body or test requests
        if (string.IsNullOrWhiteSpace(rawBody))
        {
            _logger.LogWarning("PayOS Webhook received an empty request body. Returning 200 OK for registration validation.");
            return Ok(new { Message = "Webhook endpoint reachable" });
        }

        bool isTestPayload = rawBody.Contains("test", StringComparison.OrdinalIgnoreCase) || 
                             rawBody.Contains("ping", StringComparison.OrdinalIgnoreCase) || 
                             rawBody.Contains("validation", StringComparison.OrdinalIgnoreCase) || 
                             rawBody.Trim() == "{}";

        PayOS.Models.Webhooks.Webhook? webhookRequest = null;
        try
        {
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            webhookRequest = System.Text.Json.JsonSerializer.Deserialize<PayOS.Models.Webhooks.Webhook>(rawBody, options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PayOS Webhook failed to parse request body.");
            if (isTestPayload)
            {
                return Ok(new { Message = "Webhook endpoint reachable" });
            }
            return BadRequest(new { Message = "Invalid request payload format." });
        }

        if (webhookRequest == null || string.IsNullOrWhiteSpace(webhookRequest.Signature))
        {
            _logger.LogWarning("PayOS Webhook received a non-empty payload missing signature.");
            if (isTestPayload || webhookRequest == null)
            {
                _logger.LogInformation("Non-empty payload identified as test/validation. Returning 200 OK.");
                return Ok(new { Message = "Webhook endpoint reachable" });
            }
            
            _logger.LogWarning("Non-empty payload is not identified as test/validation. Returning 400 BadRequest.");
            return BadRequest(new { Message = "Missing signature." });
        }

        // 4. Process real webhook logic
        var result = await _service.HandlePayOsWebhookAsync(webhookRequest);
        if (!result.Success)
        {
            _logger.LogWarning("PayOS Webhook verification or processing failed: {Message}", result.Message);
            return BadRequest(result);
        }

        return Ok(result);
    }
}
