using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<string>> RegisterCustomerAsync();
    Task<ApiResponse<string>> RegisterPtAsync();
    Task<ApiResponse<string>> LoginAsync();
    Task<ApiResponse<string>> RefreshTokenAsync();
    Task<ApiResponse<string>> LogoutAsync();
    Task<ApiResponse<string>> ForgotPasswordAsync();
    Task<ApiResponse<string>> ResetPasswordAsync();
    Task<ApiResponse<string>> VerifyEmailAsync();
    Task<ApiResponse<string>> GetMeAsync();
}
