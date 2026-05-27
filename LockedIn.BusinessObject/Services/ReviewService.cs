using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;

namespace LockedIn.BusinessObject.Services;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<string>> CreateReviewAsync()
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

    public async Task<ApiResponse<string>> GetMyReviewsAsync()
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

    public async Task<ApiResponse<string>> GetReviewsByPtAsync(Guid ptProfileId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

    public async Task<ApiResponse<string>> UpdateReviewAsync(Guid reviewId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

    public async Task<ApiResponse<string>> DeleteReviewAsync(Guid reviewId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

}
