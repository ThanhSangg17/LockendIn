using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.DTOs.PtProfiles;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/pts")]
[Authorize]
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
    public async Task<IActionResult> UpdateMyPtProfileAsync([FromBody] UpdatePtProfileRequest request)
    {
        var result = await _service.UpdateMyPtProfileAsync(request);
        return Ok(result);
    }

    [HttpPost("me/documents")]
    public async Task<IActionResult> UploadDocumentAsync([FromBody] UploadPtDocumentRequest request)
    {
        var result = await _service.UploadDocumentAsync(request);
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
