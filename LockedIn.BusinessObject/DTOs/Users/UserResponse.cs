using System;

namespace LockedIn.BusinessObject.DTOs.Users;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public int Role { get; set; }
    public int Status { get; set; }
    public bool EmailVerified { get; set; }
}
