using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.DTOs.Packages;

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
        return await Task.FromResult(ApiResponse<PackageResponse>.Ok(new PackageResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<IReadOnlyList<PackageResponse>>> GetMyPackagesAsync()
    {
        return await Task.FromResult(ApiResponse<IReadOnlyList<PackageResponse>>.Ok(new List<PackageResponse>(), "Not implemented yet"));
    }

    public async Task<ApiResponse<PackageResponse>> GetPackageByIdAsync(Guid packageId)
    {
        return await Task.FromResult(ApiResponse<PackageResponse>.Ok(new PackageResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<PackageResponse>> UpdatePackageAsync(Guid packageId, UpdatePackageRequest request)
    {
        return await Task.FromResult(ApiResponse<PackageResponse>.Ok(new PackageResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<PackageResponse>> HidePackageAsync(Guid packageId)
    {
        return await Task.FromResult(ApiResponse<PackageResponse>.Ok(new PackageResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<PackageResponse>> ShowPackageAsync(Guid packageId)
    {
        return await Task.FromResult(ApiResponse<PackageResponse>.Ok(new PackageResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<string>> DeletePackageAsync(Guid packageId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok(string.Empty, "Not implemented yet"));
    }
}
