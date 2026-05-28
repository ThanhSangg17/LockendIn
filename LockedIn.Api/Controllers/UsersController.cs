using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.DTOs.Users;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
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
    public async Task<IActionResult> UpdateMyProfileAsync([FromBody] UpdateUserRequest request)
    {
        var result = await _service.UpdateMyProfileAsync(request);
        return Ok(result);
    }

    [HttpPut("me/avatar")]
    public async Task<IActionResult> UpdateAvatarAsync([FromBody] UpdateAvatarRequest request)
    {
        var result = await _service.UpdateAvatarAsync(request);
        return Ok(result);
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequest request)
    {
        var result = await _service.ChangePasswordAsync(request);
        return Ok(result);
    }
}
