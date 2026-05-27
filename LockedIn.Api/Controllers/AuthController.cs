using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.DTOs.Auth;

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
    public async Task<IActionResult> RegisterCustomerAsync([FromBody] RegisterCustomerRequest request)
    {
        var result = await _service.RegisterCustomerAsync(request);
        return Ok(result);
    }

    [HttpPost("register/pt")]
    public async Task<IActionResult> RegisterPtAsync([FromBody] RegisterPtRequest request)
    {
        var result = await _service.RegisterPtAsync(request);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
    {
        var result = await _service.LoginAsync(request);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest request)
    {
        var result = await _service.RefreshTokenAsync(request);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        var result = await _service.LogoutAsync();
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request)
    {
        var result = await _service.ForgotPasswordAsync(request);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
    {
        var result = await _service.ResetPasswordAsync(request);
        return Ok(result);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmailAsync([FromBody] VerifyEmailRequest request)
    {
        var result = await _service.VerifyEmailAsync(request);
        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMeAsync()
    {
        var result = await _service.GetMeAsync();
        return Ok(result);
    }
}
