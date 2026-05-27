using System;

namespace LockedIn.BusinessObject.DTOs.Auth;

public class RegisterCustomerRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
}
