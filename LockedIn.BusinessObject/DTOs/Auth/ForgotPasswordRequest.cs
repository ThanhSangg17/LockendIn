using System;

namespace LockedIn.BusinessObject.DTOs.Auth;

public class ForgotPasswordRequest
{
    public string Email { get; set; } = null!;
}
