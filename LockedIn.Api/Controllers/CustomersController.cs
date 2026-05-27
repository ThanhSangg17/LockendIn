using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _service;

    public CustomersController(ICustomerService service)
    {
        _service = service;
    }

    [HttpGet("me/profile")]
    public async Task<IActionResult> GetMyCustomerProfileAsync()
    {
        var result = await _service.GetMyCustomerProfileAsync();
        return Ok(result);
    }

    [HttpPut("me/profile")]
    public async Task<IActionResult> UpdateMyCustomerProfileAsync()
    {
        var result = await _service.UpdateMyCustomerProfileAsync();
        return Ok(result);
    }

}
