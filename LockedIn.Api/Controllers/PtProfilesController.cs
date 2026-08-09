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

    [HttpPut("me/qr-code")]
    public async Task<IActionResult> UpdateMyQrCodeAsync([FromBody] UpdatePtQrCodeRequest request)
    {
        var result = await _service.UpdateMyQrCodeAsync(request);
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

    [HttpPost("me/verification/submit")]
    public async Task<IActionResult> SubmitVerificationAsync()
    {
        var result = await _service.SubmitVerificationAsync();
        return Ok(result);
    }

    [HttpPost("me/profile-edit-requests")]
    public async Task<IActionResult> SubmitProfileEditRequestAsync([FromBody] LockedIn.BusinessObject.DTOs.PtProfile.SubmitProfileEditRequest request)
    {
        var result = await _service.SubmitProfileEditRequestAsync(request);
        return Ok(result);
    }

    [HttpGet("me/profile-edit-requests")]
    public async Task<IActionResult> GetMyProfileEditRequestsAsync()
    {
        var result = await _service.GetMyProfileEditRequestsAsync();
        return Ok(result);
    }
}
