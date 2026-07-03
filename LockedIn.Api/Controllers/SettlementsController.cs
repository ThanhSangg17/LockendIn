using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/settlements")]
[Authorize]
public class SettlementsController : ControllerBase
{
    private readonly ISettlementService _service;

    public SettlementsController(ISettlementService service)
    {
        _service = service;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMySettlementsAsync(
        [FromQuery] LockedIn.BusinessObject.Common.PaginationRequest request,
        [FromQuery] int? settlementStatus,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var result = await _service.GetMySettlementsAsync(request, settlementStatus, startDate, endDate);
        return Ok(result);
    }

    [HttpGet("{settlementId}")]
    public async Task<IActionResult> GetSettlementByIdAsync(Guid settlementId)
    {
        var result = await _service.GetSettlementByIdAsync(settlementId);
        return Ok(result);
    }

}
