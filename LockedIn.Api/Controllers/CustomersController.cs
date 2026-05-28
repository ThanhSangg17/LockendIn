using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.DTOs.Customers;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
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
    public async Task<IActionResult> UpdateMyCustomerProfileAsync([FromBody] UpdateCustomerProfileRequest request)
    {
        var result = await _service.UpdateMyCustomerProfileAsync(request);
        return Ok(result);
    }
}
