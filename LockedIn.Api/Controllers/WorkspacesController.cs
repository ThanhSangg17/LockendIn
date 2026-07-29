using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.DTOs.Workspaces;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/workspaces")]
[Authorize]
public class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceService _service;

    public WorkspacesController(IWorkspaceService service)
    {
        _service = service;
    }

    [HttpGet("booking/{bookingId}")]
    public async Task<IActionResult> GetWorkspaceByBookingAsync(Guid bookingId)
    {
        var result = await _service.GetWorkspaceByBookingAsync(bookingId);
        return Ok(result);
    }

    [HttpGet("{workspaceId}")]
    public async Task<IActionResult> GetWorkspaceByIdAsync(Guid workspaceId)
    {
        var result = await _service.GetWorkspaceByIdAsync(workspaceId);
        return Ok(result);
    }

    [HttpPut("{workspaceId}/course-note")]
    public async Task<IActionResult> UpdateCourseNoteAsync(Guid workspaceId, [FromBody] UpdateCourseNoteRequest request)
    {
        var result = await _service.UpdateCourseNoteAsync(workspaceId, request);
        return Ok(result);
    }

    [HttpPost("{workspaceId}/sessions")]
    public async Task<IActionResult> CreateSessionAsync(Guid workspaceId, [FromBody] CreateWorkspaceSessionRequest request)
    {
        var result = await _service.CreateSessionAsync(workspaceId, request);
        return Ok(result);
    }

    [HttpGet("{workspaceId}/sessions")]
    public async Task<IActionResult> GetWorkspaceProgressAsync(Guid workspaceId)
    {
        var result = await _service.GetWorkspaceProgressAsync(workspaceId);
        return Ok(result);
    }

    #region Proposal Endpoints

    [HttpPost("{workspaceId}/proposals")]
    public async Task<IActionResult> CreateProposalAsync(Guid workspaceId, [FromBody] CreateSessionProposalRequest request)
    {
        var result = await _service.CreateProposalAsync(workspaceId, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{workspaceId}/proposals/active")]
    public async Task<IActionResult> GetActiveProposalAsync(Guid workspaceId)
    {
        var result = await _service.GetActiveProposalAsync(workspaceId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpGet("{workspaceId}/proposals/{proposalId}/availability")]
    public async Task<IActionResult> GetProposalAvailabilityAsync(Guid workspaceId, Guid proposalId)
    {
        var result = await _service.GetProposalAvailabilityAsync(workspaceId, proposalId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{workspaceId}/proposals/{proposalId}/revoke")]
    public async Task<IActionResult> RevokeProposalAsync(Guid workspaceId, Guid proposalId)
    {
        var result = await _service.RevokeProposalAsync(workspaceId, proposalId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{workspaceId}/proposals/{proposalId}/select")]
    public async Task<IActionResult> SelectProposalSlotAsync(Guid workspaceId, Guid proposalId, [FromBody] SelectSlotRequest request)
    {
        var result = await _service.SelectProposalSlotAsync(workspaceId, proposalId, request);
        if (!result.Success)
        {
            if (result.Message != null && result.Message.Contains("409"))
                return StatusCode(409, result);
            return BadRequest(result);
        }
        return Ok(result);
    }

    #endregion

    #region Check-in & Complete Endpoints

    [HttpPost("{workspaceId}/sessions/{sessionId}/check-in")]
    public async Task<IActionResult> CheckInSessionAsync(Guid workspaceId, Guid sessionId)
    {
        var result = await _service.CheckInSessionAsync(workspaceId, sessionId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{workspaceId}/sessions/{sessionId}/complete")]
    public async Task<IActionResult> CompleteSessionAsync(Guid workspaceId, Guid sessionId)
    {
        var result = await _service.CompleteSessionAsync(workspaceId, sessionId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    #endregion
}
