using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/marketplace")]
public class MarketplaceController : ControllerBase
{
    private readonly IMarketplaceService _service;

    public MarketplaceController(IMarketplaceService service)
    {
        _service = service;
    }

    [HttpGet("pts")]
    public async Task<IActionResult> GetPtsAsync()
    {
        var result = await _service.GetPtsAsync();
        return Ok(result);
    }

    [HttpGet("pts/{ptProfileId}")]
    public async Task<IActionResult> GetPtDetailAsync(Guid ptProfileId)
    {
        var result = await _service.GetPtDetailAsync(ptProfileId);
        return Ok(result);
    }

    [HttpGet("pts/{ptProfileId}/packages")]
    public async Task<IActionResult> GetPtPackagesAsync(Guid ptProfileId)
    {
        var result = await _service.GetPtPackagesAsync(ptProfileId);
        return Ok(result);
    }

    [HttpGet("pts/{ptProfileId}/reviews")]
    public async Task<IActionResult> GetPtReviewsAsync(Guid ptProfileId)
    {
        var result = await _service.GetPtReviewsAsync(ptProfileId);
        return Ok(result);
    }

}
