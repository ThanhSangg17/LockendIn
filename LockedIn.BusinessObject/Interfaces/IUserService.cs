using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.Interfaces;

public interface IUserService
{
    Task<ApiResponse<string>> GetMyProfileAsync();
    Task<ApiResponse<string>> UpdateMyProfileAsync();
    Task<ApiResponse<string>> UpdateAvatarAsync();
    Task<ApiResponse<string>> ChangePasswordAsync();
}
