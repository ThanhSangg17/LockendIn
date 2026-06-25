using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.DTOs.Auth;
using Google.Apis.Auth;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public AuthService(
        IUnitOfWork unitOfWork, 
        IConfiguration configuration, 
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _currentUserService = currentUserService;
        _emailService = emailService;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterCustomerAsync(RegisterCustomerRequest request)
    {
        request.Email = request.Email?.Trim().ToLowerInvariant()!;
        request.FullName = request.FullName?.Trim()!;
        request.Phone = request.Phone?.Trim()!;

        var validationError = ValidateRegisterInput(request.Email, request.Password, request.FullName);
        if (validationError != null)
        {
            return ApiResponse<AuthResponse>.Fail(validationError);
        }

        var emailExists = await _unitOfWork.Users.Query()
            .AnyAsync(u => u.Email == request.Email);

        if (emailExists)
        {
            return ApiResponse<AuthResponse>.Fail("Email already exists.");
        }

        var phoneExists = await _unitOfWork.Users.Query()
            .AnyAsync(u => u.Phone == request.Phone);

        if (phoneExists)
        {
            return ApiResponse<AuthResponse>.Fail("Phone number already exists.");
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = hashedPassword,
                FullName = request.FullName,
                Phone = request.Phone,
                Role = 1, // Customer
                Status = 1, // Active
                EmailVerified = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Users.AddAsync(user);

            var customerProfile = new CustomerProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.CustomerProfiles.AddAsync(customerProfile);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            try
            {
                await _emailService.SendVerificationEmailAsync(user.Id, user.Email, user.FullName);
            }
            catch (Exception)
            {
                // Ignore email sending exceptions to complete registration successfully
            }

            return ApiResponse<AuthResponse>.Ok(new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                AccessToken = "",
                RefreshToken = ""
            }, "Registration successful. Please check your email to verify your account.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<AuthResponse>.Fail($"Registration failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AuthResponse>> RegisterPtAsync(RegisterPtRequest request)
    {
        request.Email = request.Email?.Trim().ToLowerInvariant()!;
        request.FullName = request.FullName?.Trim()!;
        request.Phone = request.Phone?.Trim()!;

        var validationError = ValidateRegisterInput(request.Email, request.Password, request.FullName);
        if (validationError != null)
        {
            return ApiResponse<AuthResponse>.Fail(validationError);
        }

        var emailExists = await _unitOfWork.Users.Query()
            .AnyAsync(u => u.Email == request.Email);

        if (emailExists)
        {
            return ApiResponse<AuthResponse>.Fail("Email already exists.");
        }

        var phoneExists = await _unitOfWork.Users.Query()
            .AnyAsync(u => u.Phone == request.Phone);

        if (phoneExists)
        {
            return ApiResponse<AuthResponse>.Fail("Phone number already exists.");
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = hashedPassword,
                FullName = request.FullName,
                Phone = request.Phone,
                Role = 2, // PT
                Status = 1, // Active
                EmailVerified = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Users.AddAsync(user);

            var ptProfile = new PtProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Bio = request.Bio,
                Specialization = request.Specialization,
                ExperienceYears = request.ExperienceYears,
                VerificationStatus = 1,
                AverageRating = 0,
                TotalReviews = 0,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.PtProfiles.AddAsync(ptProfile);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            try
            {
                await _emailService.SendVerificationEmailAsync(user.Id, user.Email, user.FullName);
            }
            catch (Exception)
            {
                // Ignore email sending exceptions to complete registration successfully
            }

            return ApiResponse<AuthResponse>.Ok(new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                AccessToken = "",
                RefreshToken = ""
            }, "Registration successful. Please check your email to verify your account.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<AuthResponse>.Fail($"Registration failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        request.Email = request.Email?.Trim().ToLowerInvariant()!;

        var validationError = ValidateLoginInput(request.Email, request.Password);
        if (validationError != null)
        {
            return ApiResponse<AuthResponse>.Fail(validationError);
        }

        var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || user.IsDeleted)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid email or password");
        }

        if (user.Status != 1)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid email or password");
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid email or password");
        }

        if (!user.EmailVerified)
        {
            return ApiResponse<AuthResponse>.Fail("Please verify your email before logging in.");
        }

        var accessToken = GenerateAccessToken(user);
        var rawRefreshToken = GenerateRefreshToken();
        var refreshTokenHash = HashRefreshToken(rawRefreshToken);

        var refreshTokenExpiresDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiresDays),
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken
        }, "Login successful.");
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return ApiResponse<AuthResponse>.Fail("Refresh token is required.");
        }

        var hashedToken = HashRefreshToken(request.RefreshToken);

        var tokenRecord = await _unitOfWork.RefreshTokens.Query()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hashedToken);

        if (tokenRecord == null)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid refresh token.");
        }

        if (tokenRecord.RevokedAt != null)
        {
            return ApiResponse<AuthResponse>.Fail("Refresh token has been revoked.");
        }

        if (tokenRecord.ExpiresAt <= DateTime.UtcNow)
        {
            return ApiResponse<AuthResponse>.Fail("Refresh token has expired.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            tokenRecord.RevokedAt = DateTime.UtcNow;
            _unitOfWork.RefreshTokens.Update(tokenRecord);

            var newRawRefreshToken = GenerateRefreshToken();
            var newRefreshTokenHash = HashRefreshToken(newRawRefreshToken);

            var refreshTokenExpiresDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");
            var newRefreshTokenRecord = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = tokenRecord.UserId,
                TokenHash = newRefreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiresDays),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenRecord);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var newAccessToken = GenerateAccessToken(tokenRecord.User);

            return ApiResponse<AuthResponse>.Ok(new AuthResponse
            {
                UserId = tokenRecord.User.Id,
                Email = tokenRecord.User.Email,
                FullName = tokenRecord.User.FullName,
                Role = tokenRecord.User.Role,
                AccessToken = newAccessToken,
                RefreshToken = newRawRefreshToken
            }, "Token refreshed successfully.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<AuthResponse>.Fail($"Failed to refresh token: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> LogoutAsync()
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Logged out successfully", "Logged out successfully"));
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
        try
        {
            var bytes = Microsoft.AspNetCore.WebUtilities.Base64UrlTextEncoder.Decode(request.Token);
            var tokenRaw = Encoding.UTF8.GetString(bytes);
            var parts = tokenRaw.Split("||");
            if (parts.Length != 2)
            {
                return ApiResponse<string>.Fail("Invalid verification token format.");
            }

            var payload = parts[0];
            var signature = parts[1];

            var secretKey = _configuration["Jwt:SecretKey"]!;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var expectedHashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var expectedSignature = Convert.ToBase64String(expectedHashBytes);
                if (signature != expectedSignature)
                {
                    return ApiResponse<string>.Fail("Verification token signature is invalid.");
                }
            }

            var payloadParts = payload.Split('|');
            if (payloadParts.Length != 3)
            {
                return ApiResponse<string>.Fail("Invalid verification token payload.");
            }

            if (!Guid.TryParse(payloadParts[0], out var tokenUserId) || tokenUserId != request.UserId)
            {
                return ApiResponse<string>.Fail("User ID mismatch.");
            }

            if (!long.TryParse(payloadParts[2], out var expirationTicks))
            {
                return ApiResponse<string>.Fail("Invalid verification token expiration.");
            }

            var expirationUtc = new DateTime(expirationTicks, DateTimeKind.Utc);
            if (DateTime.UtcNow > expirationUtc)
            {
                return ApiResponse<string>.Fail("Verification link has expired.");
            }

            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            if (user == null || user.IsDeleted)
            {
                return ApiResponse<string>.Fail("User not found.");
            }

            if (user.EmailVerified)
            {
                return ApiResponse<string>.Ok("Email is already verified.", "Email is already verified.");
            }

            user.EmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<string>.Ok("Email verified successfully.", "Email verified successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail($"Email verification failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CurrentUserResponse>> GetMeAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<CurrentUserResponse>.Fail("Unauthorized");
        }

        var userId = _currentUserService.UserId.Value;
        var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null || user.Status != 1)
        {
            return ApiResponse<CurrentUserResponse>.Fail("User not found");
        }

        var response = new CurrentUserResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role
        };

        return ApiResponse<CurrentUserResponse>.Ok(response, "Current user retrieved successfully.");
    }

    public async Task<ApiResponse<AuthResponse>> GoogleLoginAsync(GoogleLoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.IdToken))
        {
            return ApiResponse<AuthResponse>.Fail("Google ID token is required.");
        }

        var clientId = _configuration["Google:ClientId"] ?? _configuration["Authentication:Google:ClientId"];
        if (string.IsNullOrEmpty(clientId))
        {
            return ApiResponse<AuthResponse>.Fail("Google Client ID is not configured.");
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        }
        catch (InvalidJwtException ex)
        {
            return ApiResponse<AuthResponse>.Fail($"Invalid Google ID token: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ApiResponse<AuthResponse>.Fail($"Google token validation failed: {ex.Message}");
        }

        if (!payload.EmailVerified)
        {
            return ApiResponse<AuthResponse>.Fail("Google email is not verified.");
        }

        var email = payload.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(email))
        {
            return ApiResponse<AuthResponse>.Fail("Email claim is missing in Google token.");
        }

        var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user != null)
        {
            if (user.IsDeleted)
            {
                return ApiResponse<AuthResponse>.Fail("User account has been deleted.");
            }

            if (user.Status != (int)UserStatus.Active)
            {
                return ApiResponse<AuthResponse>.Fail("User account is inactive.");
            }

            if (user.Role != (int)UserRole.Customer)
            {
                return ApiResponse<AuthResponse>.Fail("Google Login currently supports Customer accounts only.");
            }

            if (!user.EmailVerified)
            {
                user.EmailVerified = true;
                user.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();
            }
        }
        else
        {
            if (request.Role == null)
            {
                return ApiResponse<AuthResponse>.Fail("Role is required for first-time Google registration.");
            }

            if (request.Role != (int)UserRole.Customer)
            {
                return ApiResponse<AuthResponse>.Fail("Google Login currently supports Customer accounts only.");
            }

            var secureRandomPassword = Guid.NewGuid().ToString();
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(secureRandomPassword);

            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = hashedPassword,
                FullName = payload.Name ?? email.Split('@')[0],
                Role = (int)UserRole.Customer,
                Status = (int)UserStatus.Active,
                EmailVerified = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                AvatarUrl = payload.Picture
            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Users.AddAsync(user);

                var customerProfile = new CustomerProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.CustomerProfiles.AddAsync(customerProfile);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<AuthResponse>.Fail($"Google registration failed: {ex.Message}");
            }
        }

        var accessToken = GenerateAccessToken(user);
        var rawRefreshToken = GenerateRefreshToken();
        var refreshTokenHash = HashRefreshToken(rawRefreshToken);

        var refreshTokenExpiresDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiresDays),
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken
        }, "Login successful.");
    }

    #region Helper Methods

    private string GenerateAccessToken(User user)
    {
        var secretKey = _configuration["Jwt:SecretKey"]!;
        var issuer = _configuration["Jwt:Issuer"]!;
        var audience = _configuration["Jwt:Audience"]!;
        var expirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("role", user.Role.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("name", user.FullName)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string HashRefreshToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashedBytes);
    }

    private string? ValidateRegisterInput(string email, string password, string fullName)
    {
        return null;
    }

    private string? ValidateLoginInput(string email, string password)
    {
        return null;
    }

    #endregion
}

