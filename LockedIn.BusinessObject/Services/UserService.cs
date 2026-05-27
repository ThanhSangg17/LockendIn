using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.DTOs.Users;

namespace LockedIn.BusinessObject.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<UserResponse>> GetMyProfileAsync()
    {
        return await Task.FromResult(ApiResponse<UserResponse>.Ok(new UserResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<UserResponse>> UpdateMyProfileAsync(UpdateUserRequest request)
    {
        return await Task.FromResult(ApiResponse<UserResponse>.Ok(new UserResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<UserResponse>> UpdateAvatarAsync(UpdateAvatarRequest request)
    {
        return await Task.FromResult(ApiResponse<UserResponse>.Ok(new UserResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordRequest request)
    {
        return await Task.FromResult(ApiResponse<string>.Ok(string.Empty, "Not implemented yet"));
    }
}
