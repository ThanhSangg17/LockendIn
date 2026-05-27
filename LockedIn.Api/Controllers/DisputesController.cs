using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/disputes")]
public class DisputesController : ControllerBase
{
    private readonly IDisputeService _service;

    public DisputesController(IDisputeService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDisputeAsync()
    {
        var result = await _service.CreateDisputeAsync();
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
    public async Task<IActionResult> UploadEvidenceAsync(Guid disputeId)
    {
        var result = await _service.UploadEvidenceAsync(disputeId);
        return Ok(result);
    }

}
