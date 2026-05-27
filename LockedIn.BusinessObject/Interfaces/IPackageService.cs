using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.Interfaces;

public interface IPackageService
{
    Task<ApiResponse<string>> CreatePackageAsync();
    Task<ApiResponse<string>> GetMyPackagesAsync();
    Task<ApiResponse<string>> GetPackageByIdAsync(Guid packageId);
    Task<ApiResponse<string>> UpdatePackageAsync(Guid packageId);
    Task<ApiResponse<string>> HidePackageAsync(Guid packageId);
    Task<ApiResponse<string>> ShowPackageAsync(Guid packageId);
    Task<ApiResponse<string>> DeletePackageAsync(Guid packageId);
}
