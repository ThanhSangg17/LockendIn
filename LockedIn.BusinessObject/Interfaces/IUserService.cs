using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Users;

namespace LockedIn.BusinessObject.Interfaces;

public interface IUserService
{
    Task<ApiResponse<UserResponse>> GetMyProfileAsync();
    Task<ApiResponse<UserResponse>> UpdateMyProfileAsync(UpdateUserRequest request);
    Task<ApiResponse<UserResponse>> UpdateAvatarAsync(UpdateAvatarRequest request);
    Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordRequest request);
}
