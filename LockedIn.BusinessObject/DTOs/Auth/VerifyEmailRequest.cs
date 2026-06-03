using System;

namespace LockedIn.BusinessObject.DTOs.Auth;

public class VerifyEmailRequest
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
}
