using System;

namespace LockedIn.BusinessObject.DTOs.Auth;

public class CurrentUserResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public int Role { get; set; }
}
