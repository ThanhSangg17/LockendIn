using System;

namespace LockedIn.BusinessObject.DTOs.Users;

public class UpdateUserRequest
{
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
}
