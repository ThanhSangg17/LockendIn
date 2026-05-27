using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/workspaces")]
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
    public async Task<IActionResult> UpdateCourseNoteAsync(Guid workspaceId)
    {
        var result = await _service.UpdateCourseNoteAsync(workspaceId);
        return Ok(result);
    }

}
