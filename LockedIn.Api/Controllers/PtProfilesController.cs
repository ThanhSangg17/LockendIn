using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/pts")]
public class PtProfilesController : ControllerBase
{
    private readonly IPtProfileService _service;

    public PtProfilesController(IPtProfileService service)
    {
        _service = service;
    }

    [HttpGet("me/profile")]
    public async Task<IActionResult> GetMyPtProfileAsync()
    {
        var result = await _service.GetMyPtProfileAsync();
        return Ok(result);
    }

    [HttpPut("me/profile")]
    public async Task<IActionResult> UpdateMyPtProfileAsync()
    {
        var result = await _service.UpdateMyPtProfileAsync();
        return Ok(result);
    }

    [HttpPost("me/documents")]
    public async Task<IActionResult> UploadDocumentAsync()
    {
        var result = await _service.UploadDocumentAsync();
        return Ok(result);
    }

    [HttpGet("me/documents")]
    public async Task<IActionResult> GetMyDocumentsAsync()
    {
        var result = await _service.GetMyDocumentsAsync();
        return Ok(result);
    }

    [HttpDelete("me/documents/{documentId}")]
    public async Task<IActionResult> DeleteDocumentAsync(Guid documentId)
    {
        var result = await _service.DeleteDocumentAsync(documentId);
        return Ok(result);
    }

}
