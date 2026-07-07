using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.AddonOrders;
using LockedIn.BusinessObject.Enums;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.Api.Controllers;

[Route("api/addon-orders")]
[ApiController]
[Authorize]
public class AddonOrdersController : ControllerBase
{
    private readonly IAddonOrderService _addonOrderService;

    public AddonOrdersController(IAddonOrderService addonOrderService)
    {
        _addonOrderService = addonOrderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateAddonOrderRequest request)
    {
        var response = await _addonOrderService.CreateOrderAsync(request);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpPost("{orderId}/payment-link")]
    public async Task<IActionResult> GetOrRecreatePaymentLink(Guid orderId)
    {
        var response = await _addonOrderService.GetOrRecreatePaymentLinkAsync(orderId);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] AddonOrderStatus? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var pagination = new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize };
        var response = await _addonOrderService.GetMyOrdersAsync(pagination, status, startDate, endDate);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }
}
