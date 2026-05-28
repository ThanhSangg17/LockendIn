using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Packages;

namespace LockedIn.BusinessObject.Interfaces;

public interface IPackageService
{
    Task<ApiResponse<PackageResponse>> CreatePackageAsync(CreatePackageRequest request);
    Task<ApiResponse<IReadOnlyList<PackageResponse>>> GetMyPackagesAsync();
    Task<ApiResponse<PackageResponse>> GetPackageByIdAsync(Guid packageId);
    Task<ApiResponse<PackageResponse>> UpdatePackageAsync(Guid packageId, UpdatePackageRequest request);
    Task<ApiResponse<string>> HidePackageAsync(Guid packageId);
    Task<ApiResponse<string>> ShowPackageAsync(Guid packageId);
    Task<ApiResponse<string>> DeletePackageAsync(Guid packageId);
}
