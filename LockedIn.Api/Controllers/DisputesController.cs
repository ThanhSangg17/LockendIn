using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.DTOs.Disputes;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/disputes")]
[Authorize]
public class DisputesController : ControllerBase
{
    private readonly IDisputeService _service;

    public DisputesController(IDisputeService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDisputeAsync([FromBody] CreateDisputeRequest request)
    {
        var result = await _service.CreateDisputeAsync(request);
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyDisputesAsync()
    {
        var result = await _service.GetMyDisputesAsync();
        return Ok(result);
    }

    [HttpGet("{disputeId}")]
    public async Task<IActionResult> GetDisputeByIdAsync(Guid disputeId)
    {
        var result = await _service.GetDisputeByIdAsync(disputeId);
        return Ok(result);
    }

    [HttpPost("{disputeId}/evidences")]
    public async Task<IActionResult> UploadEvidenceAsync(Guid disputeId, [FromBody] UploadDisputeEvidenceRequest request)
    {
        var result = await _service.UploadEvidenceAsync(disputeId, request);
        return Ok(result);
    }
}
