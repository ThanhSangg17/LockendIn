using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/conversations")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _service;

    public ConversationsController(IConversationService service)
    {
        _service = service;
    }

    [HttpGet("workspace/{workspaceId}")]
    public async Task<IActionResult> GetConversationByWorkspaceAsync(Guid workspaceId)
    {
        var result = await _service.GetConversationByWorkspaceAsync(workspaceId);
        return Ok(result);
    }

    [HttpPost("booking/{bookingId}")]
    public async Task<IActionResult> CreateConversationByBookingAsync(Guid bookingId)
    {
        var result = await _service.CreateConversationByBookingAsync(bookingId);
        return Ok(result);
    }

}
