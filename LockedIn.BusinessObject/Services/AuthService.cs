using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.DTOs.Auth;

namespace LockedIn.BusinessObject.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterCustomerAsync(RegisterCustomerRequest request)
    {
        return await Task.FromResult(ApiResponse<AuthResponse>.Ok(new AuthResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<AuthResponse>> RegisterPtAsync(RegisterPtRequest request)
    {
        return await Task.FromResult(ApiResponse<AuthResponse>.Ok(new AuthResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        return await Task.FromResult(ApiResponse<AuthResponse>.Ok(new AuthResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        return await Task.FromResult(ApiResponse<AuthResponse>.Ok(new AuthResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<string>> LogoutAsync()
    {
        return await Task.FromResult(ApiResponse<string>.Ok(string.Empty, "Not implemented yet"));
    }

    public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        return await Task.FromResult(ApiResponse<string>.Ok(string.Empty, "Not implemented yet"));
    }

    public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        return await Task.FromResult(ApiResponse<string>.Ok(string.Empty, "Not implemented yet"));
    }

    public async Task<ApiResponse<string>> VerifyEmailAsync(VerifyEmailRequest request)
    {
        return await Task.FromResult(ApiResponse<string>.Ok(string.Empty, "Not implemented yet"));
    }

    public async Task<ApiResponse<CurrentUserResponse>> GetMeAsync()
    {
        return await Task.FromResult(ApiResponse<CurrentUserResponse>.Ok(new CurrentUserResponse(), "Not implemented yet"));
    }

}
