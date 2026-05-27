using System;

namespace LockedIn.BusinessObject.DTOs.Auth;

public class RegisterPtRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Bio { get; set; }
    public string? Specialization { get; set; }
    public int ExperienceYears { get; set; }
}
