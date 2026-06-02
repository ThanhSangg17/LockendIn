using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.DTOs.Conversations;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/conversations")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _service;
    private readonly IFirebaseChatService _firebaseChatService;

    public ConversationsController(IConversationService service, IFirebaseChatService firebaseChatService)
    {
        _service = service;
        _firebaseChatService = firebaseChatService;
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

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessageAsync([FromBody] SendMessageRequest request)
    {
        var result = await _firebaseChatService.SendMessageAsync(request);
        return Ok(result);
    }

    [HttpGet("{conversationId}/messages")]
    public async Task<IActionResult> GetMessagesAsync(Guid conversationId)
    {
        var result = await _firebaseChatService.GetMessagesAsync(conversationId);
        return Ok(result);
    }

    [HttpPatch("{conversationId}/messages/read")]
    public async Task<IActionResult> MarkMessagesAsReadAsync(Guid conversationId)
    {
        var result = await _firebaseChatService.MarkMessagesAsReadAsync(conversationId);
        return Ok(result);
    }
}
