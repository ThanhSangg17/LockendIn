using System;

namespace LockedIn.BusinessObject.DTOs.Users;

public class UpdateAvatarRequest
{
    public string AvatarUrl { get; set; } = null!;
}
