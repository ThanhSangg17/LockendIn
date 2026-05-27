using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;

namespace LockedIn.BusinessObject.Services;

public class PackageService : IPackageService
{
    private readonly IUnitOfWork _unitOfWork;

    public PackageService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<string>> CreatePackageAsync()
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

    public async Task<ApiResponse<string>> GetMyPackagesAsync()
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

    public async Task<ApiResponse<string>> GetPackageByIdAsync(Guid packageId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

    public async Task<ApiResponse<string>> UpdatePackageAsync(Guid packageId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

    public async Task<ApiResponse<string>> HidePackageAsync(Guid packageId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

    public async Task<ApiResponse<string>> ShowPackageAsync(Guid packageId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

    public async Task<ApiResponse<string>> DeletePackageAsync(Guid packageId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

}
