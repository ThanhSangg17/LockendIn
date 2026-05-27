using System;

namespace LockedIn.BusinessObject.DTOs.Auth;

public class VerifyEmailRequest
{
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
}
