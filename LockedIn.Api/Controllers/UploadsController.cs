using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Uploads;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/uploads")]
[Authorize]
public class UploadsController : ControllerBase
{
    private readonly ICloudinaryService _cloudinaryService;

    public UploadsController(ICloudinaryService cloudinaryService)
    {
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost("image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string folder)
    {
        var result = await _cloudinaryService.UploadImageAsync(file, folder);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("file")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string folder)
    {
        var result = await _cloudinaryService.UploadFileAsync(file, folder);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteFile([FromQuery] string publicId)
    {
        var result = await _cloudinaryService.DeleteFileAsync(publicId);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
