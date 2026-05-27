using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.Interfaces;

public interface IReviewService
{
    Task<ApiResponse<string>> CreateReviewAsync();
    Task<ApiResponse<string>> GetMyReviewsAsync();
    Task<ApiResponse<string>> GetReviewsByPtAsync(Guid ptProfileId);
    Task<ApiResponse<string>> UpdateReviewAsync(Guid reviewId);
    Task<ApiResponse<string>> DeleteReviewAsync(Guid reviewId);
}
