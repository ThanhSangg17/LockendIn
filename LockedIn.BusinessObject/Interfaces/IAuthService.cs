using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Auth;

namespace LockedIn.BusinessObject.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> RegisterCustomerAsync(RegisterCustomerRequest request);
    Task<ApiResponse<AuthResponse>> RegisterPtAsync(RegisterPtRequest request);
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<ApiResponse<string>> LogoutAsync();
    Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<ApiResponse<string>> VerifyEmailAsync(VerifyEmailRequest request);
    Task<ApiResponse<CurrentUserResponse>> GetMeAsync();
    Task<ApiResponse<AuthResponse>> GoogleLoginAsync(GoogleLoginRequest request);
}
