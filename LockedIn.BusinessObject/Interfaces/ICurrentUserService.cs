using System;

namespace LockedIn.BusinessObject.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    int? Role { get; }
    string? FullName { get; }
    bool IsAuthenticated { get; }
}
