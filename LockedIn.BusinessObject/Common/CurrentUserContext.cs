using System;

namespace LockedIn.BusinessObject.Common;

public class CurrentUserContext
{
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public int? Role { get; set; }
    public string? FullName { get; set; }
    public bool IsAuthenticated { get; set; }
}
