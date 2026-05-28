using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.DTOs.Bookings;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;

    public BookingsController(IBookingService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBookingAsync([FromBody] CreateBookingRequest request)
    {
        var result = await _service.CreateBookingAsync(request);
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyBookingsAsync()
    {
        var result = await _service.GetMyBookingsAsync();
        return Ok(result);
    }

    [HttpGet("{bookingId}")]
    public async Task<IActionResult> GetBookingByIdAsync(Guid bookingId)
    {
        var result = await _service.GetBookingByIdAsync(bookingId);
        return Ok(result);
    }

    [HttpPost("{bookingId}/cancel")]
    public async Task<IActionResult> CancelBookingAsync(Guid bookingId)
    {
        var result = await _service.CancelBookingAsync(bookingId);
        return Ok(result);
    }

    [HttpPost("{bookingId}/accept")]
    public async Task<IActionResult> AcceptBookingAsync(Guid bookingId)
    {
        var result = await _service.AcceptBookingAsync(bookingId);
        return Ok(result);
    }

    [HttpPost("{bookingId}/reject")]
    public async Task<IActionResult> RejectBookingAsync(Guid bookingId)
    {
        var result = await _service.RejectBookingAsync(bookingId);
        return Ok(result);
    }

    [HttpPost("{bookingId}/complete")]
    public async Task<IActionResult> CompleteBookingAsync(Guid bookingId)
    {
        var result = await _service.CompleteBookingAsync(bookingId);
        return Ok(result);
    }
}
