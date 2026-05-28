using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.BusinessObject.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst(ClaimTypes.NameIdentifier) ?? user?.FindFirst("sub");
            if (claim != null && Guid.TryParse(claim.Value, out var guid))
            {
                return guid;
            }
            return null;
        }
    }

    public string? Email
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirst(ClaimTypes.Email)?.Value ?? user?.FindFirst("email")?.Value;
        }
    }

    public int? Role
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst(ClaimTypes.Role) ?? user?.FindFirst("role");
            if (claim != null && int.TryParse(claim.Value, out var roleInt))
            {
                return roleInt;
            }
            return null;
        }
    }

    public string? FullName
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirst(ClaimTypes.Name)?.Value ?? user?.FindFirst("name")?.Value;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
