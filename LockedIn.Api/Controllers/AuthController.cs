using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    [HttpPost("register/customer")]
    public async Task<IActionResult> RegisterCustomerAsync()
    {
        var result = await _service.RegisterCustomerAsync();
        return Ok(result);
    }

    [HttpPost("register/pt")]
    public async Task<IActionResult> RegisterPtAsync()
    {
        var result = await _service.RegisterPtAsync();
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync()
    {
        var result = await _service.LoginAsync();
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshTokenAsync()
    {
        var result = await _service.RefreshTokenAsync();
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        var result = await _service.LogoutAsync();
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPasswordAsync()
    {
        var result = await _service.ForgotPasswordAsync();
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync()
    {
        var result = await _service.ResetPasswordAsync();
        return Ok(result);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmailAsync()
    {
        var result = await _service.VerifyEmailAsync();
        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMeAsync()
    {
        var result = await _service.GetMeAsync();
        return Ok(result);
    }

}
