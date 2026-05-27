using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;

namespace LockedIn.BusinessObject.Services;

public class MarketplaceService : IMarketplaceService
{
    private readonly IUnitOfWork _unitOfWork;

    public MarketplaceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<string>> GetPtsAsync()
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

    public async Task<ApiResponse<string>> GetPtDetailAsync(Guid ptProfileId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

    public async Task<ApiResponse<string>> GetPtPackagesAsync(Guid ptProfileId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

    public async Task<ApiResponse<string>> GetPtReviewsAsync(Guid ptProfileId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

}
