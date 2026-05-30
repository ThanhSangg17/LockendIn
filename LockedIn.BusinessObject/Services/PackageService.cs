using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.DTOs.Packages;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Services;

public class PackageService : IPackageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public PackageService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<PackageResponse>> CreatePackageAsync(CreatePackageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiResponse<PackageResponse>.Fail("Package name is required.");
        }
        if (request.SessionCount <= 0)
        {
            return ApiResponse<PackageResponse>.Fail("Session count must be greater than 0.");
        }
        if (request.Price <= 0)
        {
            return ApiResponse<PackageResponse>.Fail("Price must be greater than 0.");
        }

        var (pt, error) = await GetCurrentPtProfileAsync(requireApproved: true);
        if (error != null)
        {
            return ApiResponse<PackageResponse>.Fail(error);
        }

        var package = new Package
        {
            Id = Guid.NewGuid(),
            PtProfileId = pt.Id,
            Name = request.Name,
            Description = request.Description,
            SessionCount = request.SessionCount,
            Price = request.Price,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Packages.AddAsync(package);

        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = _currentUserService.UserId!.Value,
                Action = "CreatePackage",
                EntityName = "Package",
                EntityId = package.Id,
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { Name = package.Name, Price = package.Price }),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
        }
        catch
        {
            // silently ignore
        }

        await _unitOfWork.SaveChangesAsync();

        var response = MapToPackageResponse(package);
        return ApiResponse<PackageResponse>.Ok(response, "Package created successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<PackageResponse>>> GetMyPackagesAsync()
    {
        var (pt, error) = await GetCurrentPtProfileAsync(requireApproved: false);
        if (error != null)
        {
            return ApiResponse<IReadOnlyList<PackageResponse>>.Fail(error);
        }

        var packages = await _unitOfWork.Packages.Query()
            .Where(p => p.PtProfileId == pt.Id && !p.IsDeleted)
            .ToListAsync();

        var response = packages.Select(MapToPackageResponse).ToList();
        return ApiResponse<IReadOnlyList<PackageResponse>>.Ok(response, "My packages retrieved successfully.");
    }

    public async Task<ApiResponse<PackageResponse>> GetPackageByIdAsync(Guid packageId)
    {
        var package = await _unitOfWork.Packages.Query()
            .FirstOrDefaultAsync(p => p.Id == packageId && !p.IsDeleted);

        if (package == null)
        {
            return ApiResponse<PackageResponse>.Fail("Package not found");
        }

        var response = MapToPackageResponse(package);
        return ApiResponse<PackageResponse>.Ok(response, "Package retrieved successfully.");
    }

    public async Task<ApiResponse<PackageResponse>> UpdatePackageAsync(Guid packageId, UpdatePackageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiResponse<PackageResponse>.Fail("Package name is required.");
        }
        if (request.SessionCount <= 0)
        {
            return ApiResponse<PackageResponse>.Fail("Session count must be greater than 0.");
        }
        if (request.Price <= 0)
        {
            return ApiResponse<PackageResponse>.Fail("Price must be greater than 0.");
        }

        var (pt, error) = await GetCurrentPtProfileAsync(requireApproved: false);
        if (error != null)
        {
            return ApiResponse<PackageResponse>.Fail(error);
        }

        var package = await _unitOfWork.Packages.Query()
            .FirstOrDefaultAsync(p => p.Id == packageId && !p.IsDeleted);

        if (package == null)
        {
            return ApiResponse<PackageResponse>.Fail("Package not found");
        }

        if (package.PtProfileId != pt.Id)
        {
            return ApiResponse<PackageResponse>.Fail("You do not own this package.");
        }

        package.Name = request.Name;
        package.Description = request.Description;
        package.SessionCount = request.SessionCount;
        package.Price = request.Price;
        package.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Packages.Update(package);

        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = _currentUserService.UserId!.Value,
                Action = "UpdatePackage",
                EntityName = "Package",
                EntityId = package.Id,
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { Name = package.Name, Price = package.Price }),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
        }
        catch
        {
            // silently ignore
        }

        await _unitOfWork.SaveChangesAsync();

        var response = MapToPackageResponse(package);
        return ApiResponse<PackageResponse>.Ok(response, "Package updated successfully.");
    }

    public async Task<ApiResponse<string>> HidePackageAsync(Guid packageId)
    {
        var (pt, error) = await GetCurrentPtProfileAsync(requireApproved: false);
        if (error != null)
        {
            return ApiResponse<string>.Fail(error);
        }

        var package = await _unitOfWork.Packages.Query()
            .FirstOrDefaultAsync(p => p.Id == packageId && !p.IsDeleted);

        if (package == null)
        {
            return ApiResponse<string>.Fail("Package not found");
        }

        if (package.PtProfileId != pt.Id)
        {
            return ApiResponse<string>.Fail("You do not own this package.");
        }

        package.IsActive = false;
        package.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Packages.Update(package);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<string>.Ok("Package hidden successfully.", "Package hidden successfully.");
    }

    public async Task<ApiResponse<string>> ShowPackageAsync(Guid packageId)
    {
        var (pt, error) = await GetCurrentPtProfileAsync(requireApproved: true);
        if (error != null)
        {
            return ApiResponse<string>.Fail(error);
        }

        var package = await _unitOfWork.Packages.Query()
            .FirstOrDefaultAsync(p => p.Id == packageId && !p.IsDeleted);

        if (package == null)
        {
            return ApiResponse<string>.Fail("Package not found");
        }

        if (package.PtProfileId != pt.Id)
        {
            return ApiResponse<string>.Fail("You do not own this package.");
        }

        package.IsActive = true;
        package.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Packages.Update(package);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<string>.Ok("Package shown successfully.", "Package shown successfully.");
    }

    public async Task<ApiResponse<string>> DeletePackageAsync(Guid packageId)
    {
        var (pt, error) = await GetCurrentPtProfileAsync(requireApproved: false);
        if (error != null)
        {
            return ApiResponse<string>.Fail(error);
        }

        var package = await _unitOfWork.Packages.Query()
            .FirstOrDefaultAsync(p => p.Id == packageId && !p.IsDeleted);

        if (package == null)
        {
            return ApiResponse<string>.Fail("Package not found");
        }

        if (package.PtProfileId != pt.Id)
        {
            return ApiResponse<string>.Fail("You do not own this package.");
        }

        package.IsDeleted = true;
        package.IsActive = false;
        package.DeletedAt = DateTime.UtcNow;

        _unitOfWork.Packages.Update(package);

        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = _currentUserService.UserId!.Value,
                Action = "DeletePackage",
                EntityName = "Package",
                EntityId = package.Id,
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { Name = package.Name, Price = package.Price }),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
        }
        catch
        {
            // silently ignore
        }

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<string>.Ok("Package deleted successfully.", "Package deleted successfully.");
    }

    #region Helper Methods

    private async Task<(PtProfile? Profile, string? Error)> GetCurrentPtProfileAsync(bool requireApproved = true)
    {
        // TODO: replace with JWT current user PT profile later.
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return (null, "User is not authenticated.");
        }

        if (_currentUserService.Role != 2) // PersonalTrainer = 2
        {
            return (null, "Only personal trainers can manage packages.");
        }

        var userId = _currentUserService.UserId.Value;
        var ptProfile = await _unitOfWork.PtProfiles.Query()
            .Include(pt => pt.User)
            .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);

        if (ptProfile == null)
        {
            return (null, "PT profile not found.");
        }

        if (requireApproved && ptProfile.VerificationStatus != (int)PtVerificationStatus.Approved)
        {
            return (null, "PT profile is not approved.");
        }

        return (ptProfile, null);
    }

    private PackageResponse MapToPackageResponse(Package package)
    {
        return new PackageResponse
        {
            Id = package.Id,
            PtProfileId = package.PtProfileId,
            Name = package.Name,
            Description = package.Description,
            SessionCount = package.SessionCount,
            Price = package.Price,
            IsActive = package.IsActive
        };
    }

    #endregion
}


