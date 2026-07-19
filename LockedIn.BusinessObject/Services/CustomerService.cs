using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.DTOs.Customers;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CustomerService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<CustomerProfileResponse>> GetMyCustomerProfileAsync()
    {
        var accessError = ValidateCustomerAccess();
        if (accessError != null)
        {
            return ApiResponse<CustomerProfileResponse>.Fail(accessError);
        }

        var userId = _currentUserService.UserId!.Value;
        var profile = await _unitOfWork.CustomerProfiles.Query()
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

        if (profile == null)
        {
            return ApiResponse<CustomerProfileResponse>.Fail("Customer profile not found.");
        }

        return ApiResponse<CustomerProfileResponse>.Ok(MapToResponse(profile), "Customer profile retrieved successfully.");
    }

    public async Task<ApiResponse<CustomerProfileResponse>> UpdateMyCustomerProfileAsync(UpdateCustomerProfileRequest request)
    {
        var accessError = ValidateCustomerAccess();
        if (accessError != null)
        {
            return ApiResponse<CustomerProfileResponse>.Fail(accessError);
        }

        var userId = _currentUserService.UserId!.Value;
        var profile = await _unitOfWork.CustomerProfiles.Query()
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

        if (profile == null)
        {
            return ApiResponse<CustomerProfileResponse>.Fail("Customer profile not found.");
        }

        if (request.DateOfBirth.HasValue && request.DateOfBirth.Value.Date > DateTime.UtcNow.Date)
        {
            return ApiResponse<CustomerProfileResponse>.Fail("Date of birth cannot be in the future.");
        }

        if (request.HeightCm.HasValue && (request.HeightCm.Value <= 0 || request.HeightCm.Value > 999.99m))
        {
            return ApiResponse<CustomerProfileResponse>.Fail("Height must be greater than 0 and at most 999.99 cm.");
        }

        if (request.WeightKg.HasValue && (request.WeightKg.Value <= 0 || request.WeightKg.Value > 999.99m))
        {
            return ApiResponse<CustomerProfileResponse>.Fail("Weight must be greater than 0 and at most 999.99 kg.");
        }

        var gender = request.Gender?.Trim();
        if (!string.IsNullOrEmpty(gender) && gender.Length > 20)
        {
            return ApiResponse<CustomerProfileResponse>.Fail("Gender must not exceed 20 characters.");
        }

        var fitnessGoal = request.FitnessGoal?.Trim();
        if (!string.IsNullOrEmpty(fitnessGoal) && fitnessGoal.Length > 255)
        {
            return ApiResponse<CustomerProfileResponse>.Fail("Fitness goal must not exceed 255 characters.");
        }

        profile.DateOfBirth = request.DateOfBirth.HasValue
            ? DateOnly.FromDateTime(request.DateOfBirth.Value)
            : null;
        profile.Gender = string.IsNullOrEmpty(gender) ? null : gender;
        profile.HeightCm = request.HeightCm;
        profile.WeightKg = request.WeightKg;
        profile.FitnessGoal = string.IsNullOrEmpty(fitnessGoal) ? null : fitnessGoal;
        profile.HealthNote = string.IsNullOrWhiteSpace(request.HealthNote) ? null : request.HealthNote.Trim();
        profile.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.CustomerProfiles.Update(profile);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<CustomerProfileResponse>.Ok(MapToResponse(profile), "Customer profile updated successfully.");
    }

    #region Helper Methods

    private string? ValidateCustomerAccess()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return "Unauthorized";
        }

        if (_currentUserService.Role != (int)UserRole.Customer)
        {
            return "Only customers can access this resource.";
        }

        return null;
    }

    private static CustomerProfileResponse MapToResponse(CustomerProfile profile)
    {
        return new CustomerProfileResponse
        {
            Id = profile.Id,
            UserId = profile.UserId,
            DateOfBirth = profile.DateOfBirth.HasValue
                ? profile.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue)
                : null,
            Gender = profile.Gender,
            HeightCm = profile.HeightCm,
            WeightKg = profile.WeightKg,
            FitnessGoal = profile.FitnessGoal,
            HealthNote = profile.HealthNote
        };
    }

    #endregion
}
