using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.DTOs.Marketplace;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/marketplace")]
[AllowAnonymous]
public class MarketplaceController : ControllerBase
{
    private readonly IMarketplaceService _service;

    public MarketplaceController(IMarketplaceService service)
    {
        _service = service;
    }

    [HttpGet("pts")]
    public async Task<IActionResult> GetPtsAsync([FromQuery] PtSearchRequest request)
    {
        var result = await _service.GetPtsAsync(request);
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
