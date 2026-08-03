using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.DTOs.Users;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UserService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<UserResponse>> GetMyProfileAsync()
    {
        var user = await GetCurrentActiveUserAsync();
        if (user == null)
        {
            return ApiResponse<UserResponse>.Fail("User not found");
        }

        return ApiResponse<UserResponse>.Ok(MapToResponse(user), "Profile retrieved successfully.");
    }

    public async Task<ApiResponse<UserResponse>> UpdateMyProfileAsync(UpdateUserRequest request)
    {
        var user = await GetCurrentActiveUserAsync();
        if (user == null)
        {
            return ApiResponse<UserResponse>.Fail("User not found");
        }

        var fullName = request.FullName?.Trim();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return ApiResponse<UserResponse>.Fail("Full name is required.");
        }

        if (fullName.Length > 150)
        {
            return ApiResponse<UserResponse>.Fail("Full name must not exceed 150 characters.");
        }

        var phone = request.Phone?.Trim();
        if (!string.IsNullOrEmpty(phone))
        {
            if (phone.Length > 20)
            {
                return ApiResponse<UserResponse>.Fail("Phone number must not exceed 20 characters.");
            }

            var phoneExists = await _unitOfWork.Users.Query()
                .AnyAsync(u => u.Phone == phone && u.Id != user.Id);

            if (phoneExists)
            {
                return ApiResponse<UserResponse>.Fail("Phone number already exists.");
            }
        }

        user.FullName = fullName;
        user.Phone = string.IsNullOrEmpty(phone) ? null : phone;
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<UserResponse>.Ok(MapToResponse(user), "Profile updated successfully.");
    }

    public async Task<ApiResponse<UserResponse>> UpdateAvatarAsync(UpdateAvatarRequest request)
    {
        var user = await GetCurrentActiveUserAsync();
        if (user == null)
        {
            return ApiResponse<UserResponse>.Fail("User not found");
        }

        var avatarUrl = request.AvatarUrl?.Trim();
        if (string.IsNullOrWhiteSpace(avatarUrl))
        {
            return ApiResponse<UserResponse>.Fail("Avatar URL is required.");
        }

        if (!Uri.TryCreate(avatarUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return ApiResponse<UserResponse>.Fail("Avatar URL must be a valid absolute http/https URL.");
        }

        user.AvatarUrl = avatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<UserResponse>.Ok(MapToResponse(user), "Avatar updated successfully.");
    }

    public async Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var user = await GetCurrentActiveUserAsync();
        if (user == null)
        {
            return ApiResponse<string>.Fail("User not found");
        }

        if (string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
        {
            return ApiResponse<string>.Fail("Current password and new password are required.");
        }

        var isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
        if (!isCurrentPasswordValid)
        {
            return ApiResponse<string>.Fail("Current password is incorrect.");
        }

        var policyError = ValidatePasswordPolicy(request.NewPassword);
        if (policyError != null)
        {
            return ApiResponse<string>.Fail(policyError);
        }

        if (request.NewPassword == request.CurrentPassword)
        {
            return ApiResponse<string>.Fail("New password must be different from the current password.");
        }

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            user.PasswordHash = newPasswordHash;
            user.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);

            var now = DateTime.UtcNow;
            var activeTokens = await _unitOfWork.RefreshTokens.Query()
                .Where(t => t.UserId == user.Id && t.RevokedAt == null && t.ExpiresAt > now)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = now;
                _unitOfWork.RefreshTokens.Update(token);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return ApiResponse<string>.Ok("Password changed successfully.", "Password changed successfully. Please log in again on other devices.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<string>.Fail($"Failed to change password: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> DeleteMyAccountAsync(DeleteAccountRequest request)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<string>.Fail("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;
        var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            return ApiResponse<string>.Fail("User not found.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return ApiResponse<string>.Fail("Password is required.");
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return ApiResponse<string>.Fail("Invalid password.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // 1. Soft Delete User
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.DeletedBy = user.Id;
            _unitOfWork.Users.Update(user);

            // 2. Soft Delete Profile
            if (user.Role == (int)UserRole.Customer)
            {
                var customerProfile = await _unitOfWork.CustomerProfiles.Query()
                    .FirstOrDefaultAsync(c => c.UserId == user.Id && !c.IsDeleted);
                if (customerProfile != null)
                {
                    customerProfile.IsDeleted = true;
                    customerProfile.DeletedAt = DateTime.UtcNow;
                    customerProfile.DeletedBy = user.Id;
                    _unitOfWork.CustomerProfiles.Update(customerProfile);
                }
            }
            else if (user.Role == (int)UserRole.PersonalTrainer)
            {
                var ptProfile = await _unitOfWork.PtProfiles.Query()
                    .FirstOrDefaultAsync(p => p.UserId == user.Id && !p.IsDeleted);
                if (ptProfile != null)
                {
                    ptProfile.IsDeleted = true;
                    ptProfile.DeletedAt = DateTime.UtcNow;
                    ptProfile.DeletedBy = user.Id;
                    _unitOfWork.PtProfiles.Update(ptProfile);
                }
            }

            // 3. Revoke Refresh Tokens
            var activeTokens = await _unitOfWork.RefreshTokens.Query()
                .Where(t => t.UserId == user.Id && t.RevokedAt == null)
                .ToListAsync();
            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                _unitOfWork.RefreshTokens.Update(token);
            }

            // 4. Audit Log
            try
            {
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    ActorUserId = user.Id,
                    Action = "SelfDeleteAccount",
                    EntityName = "User",
                    EntityId = user.Id,
                    MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { Email = user.Email }),
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.AuditLogs.AddAsync(auditLog);
            }
            catch {}

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return ApiResponse<string>.Ok(user.Id.ToString(), "Account deleted successfully.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<string>.Fail($"Failed to delete account: {ex.Message}");
        }
    }

    #region Helper Methods

    private async Task<User?> GetCurrentActiveUserAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return null;
        }

        var userId = _currentUserService.UserId.Value;
        var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null || user.Status != (int)UserStatus.Active)
        {
            return null;
        }

        return user;
    }

    private static string? ValidatePasswordPolicy(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
        {
            return "Password must be at least 8 characters long.";
        }

        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
        {
            return "Password must contain at least one letter and one digit.";
        }

        return null;
    }

    private static UserResponse MapToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role,
            Status = user.Status,
            EmailVerified = user.EmailVerified
        };
    }

    #endregion
}
