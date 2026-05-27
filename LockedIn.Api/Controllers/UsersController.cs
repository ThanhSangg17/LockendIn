using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfileAsync()
    {
        var result = await _service.GetMyProfileAsync();
        return Ok(result);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfileAsync()
    {
        var result = await _service.UpdateMyProfileAsync();
        return Ok(result);
    }

    [HttpPut("me/avatar")]
    public async Task<IActionResult> UpdateAvatarAsync()
    {
        var result = await _service.UpdateAvatarAsync();
        return Ok(result);
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePasswordAsync()
    {
        var result = await _service.ChangePasswordAsync();
        return Ok(result);
    }

}
