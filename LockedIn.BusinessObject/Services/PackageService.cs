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

    public PackageService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

        var currentPt = await GetTemporaryCurrentPtProfileAsync();
        if (currentPt == null)
        {
            return ApiResponse<PackageResponse>.Fail("Temporary current PT profile not found.");
        }

        var package = new Package
        {
            Id = Guid.NewGuid(),
            PtProfileId = currentPt.Id,
            Name = request.Name,
            Description = request.Description,
            SessionCount = request.SessionCount,
            Price = request.Price,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Packages.AddAsync(package);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToPackageResponse(package);
        return ApiResponse<PackageResponse>.Ok(response, "Package created successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<PackageResponse>>> GetMyPackagesAsync()
    {
        var currentPt = await GetTemporaryCurrentPtProfileAsync();
        if (currentPt == null)
        {
            return ApiResponse<IReadOnlyList<PackageResponse>>.Fail("Temporary current PT profile not found.");
        }

        var packages = await _unitOfWork.Packages.Query()
            .Where(p => p.PtProfileId == currentPt.Id && !p.IsDeleted)
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

        var package = await _unitOfWork.Packages.Query()
            .FirstOrDefaultAsync(p => p.Id == packageId && !p.IsDeleted);

        if (package == null)
        {
            return ApiResponse<PackageResponse>.Fail("Package not found");
        }

        package.Name = request.Name;
        package.Description = request.Description;
        package.SessionCount = request.SessionCount;
        package.Price = request.Price;
        package.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Packages.Update(package);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToPackageResponse(package);
        return ApiResponse<PackageResponse>.Ok(response, "Package updated successfully.");
    }

    public async Task<ApiResponse<string>> HidePackageAsync(Guid packageId)
    {
        var package = await _unitOfWork.Packages.Query()
            .FirstOrDefaultAsync(p => p.Id == packageId && !p.IsDeleted);

        if (package == null)
        {
            return ApiResponse<string>.Fail("Package not found");
        }

        package.IsActive = false;
        package.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Packages.Update(package);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<string>.Ok("Package hidden successfully.", "Package hidden successfully.");
    }

    public async Task<ApiResponse<string>> ShowPackageAsync(Guid packageId)
    {
        var package = await _unitOfWork.Packages.Query()
            .FirstOrDefaultAsync(p => p.Id == packageId && !p.IsDeleted);

        if (package == null)
        {
            return ApiResponse<string>.Fail("Package not found");
        }

        package.IsActive = true;
        package.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Packages.Update(package);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<string>.Ok("Package shown successfully.", "Package shown successfully.");
    }

    public async Task<ApiResponse<string>> DeletePackageAsync(Guid packageId)
    {
        var package = await _unitOfWork.Packages.Query()
            .FirstOrDefaultAsync(p => p.Id == packageId && !p.IsDeleted);

        if (package == null)
        {
            return ApiResponse<string>.Fail("Package not found");
        }

        package.IsDeleted = true;
        package.IsActive = false;
        package.DeletedAt = DateTime.UtcNow;

        _unitOfWork.Packages.Update(package);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<string>.Ok("Package deleted successfully.", "Package deleted successfully.");
    }

    #region Helper Methods

    private async Task<PtProfile?> GetTemporaryCurrentPtProfileAsync()
    {
        // TODO: replace with JWT current user PT profile later.
        return await _unitOfWork.PtProfiles.Query()
            .Include(pt => pt.User)
            .FirstOrDefaultAsync(pt => !pt.IsDeleted && 
                                       !pt.User.IsDeleted && 
                                       pt.User.Status == 1 && 
                                       pt.VerificationStatus == (int)PtVerificationStatus.Approved);
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

