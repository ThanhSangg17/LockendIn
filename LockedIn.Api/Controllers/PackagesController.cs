using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.DTOs.Packages;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/packages")]
[Authorize]
public class PackagesController : ControllerBase
{
    private readonly IPackageService _service;

    public PackagesController(IPackageService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePackageAsync([FromBody] CreatePackageRequest request)
    {
        var result = await _service.CreatePackageAsync(request);
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyPackagesAsync()
    {
        var result = await _service.GetMyPackagesAsync();
        return Ok(result);
    }

    [HttpGet("{packageId}")]
    public async Task<IActionResult> GetPackageByIdAsync(Guid packageId)
    {
        var result = await _service.GetPackageByIdAsync(packageId);
        return Ok(result);
    }

    [HttpPut("{packageId}")]
    public async Task<IActionResult> UpdatePackageAsync(Guid packageId, [FromBody] UpdatePackageRequest request)
    {
        var result = await _service.UpdatePackageAsync(packageId, request);
        return Ok(result);
    }

    [HttpPatch("{packageId}/hide")]
    public async Task<IActionResult> HidePackageAsync(Guid packageId)
    {
        var result = await _service.HidePackageAsync(packageId);
        return Ok(result);
    }

    [HttpPatch("{packageId}/show")]
    public async Task<IActionResult> ShowPackageAsync(Guid packageId)
    {
        var result = await _service.ShowPackageAsync(packageId);
        return Ok(result);
    }

    [HttpDelete("{packageId}")]
    public async Task<IActionResult> DeletePackageAsync(Guid packageId)
    {
        var result = await _service.DeletePackageAsync(packageId);
        return Ok(result);
    }
}
