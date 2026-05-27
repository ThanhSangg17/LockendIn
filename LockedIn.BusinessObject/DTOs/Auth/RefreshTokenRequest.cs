using System;

namespace LockedIn.BusinessObject.DTOs.Auth;

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = null!;
}
