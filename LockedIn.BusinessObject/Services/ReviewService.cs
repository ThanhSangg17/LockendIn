using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.DTOs.Reviews;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Services;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ReviewService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<ReviewResponse>> CreateReviewAsync(CreateReviewRequest request)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<ReviewResponse>.Fail("User is not authenticated.");
        }

        if (_currentUserService.Role != (int)UserRole.Customer)
        {
            return ApiResponse<ReviewResponse>.Fail("Only customers can create reviews.");
        }

        var customerProfile = await GetCurrentCustomerProfileAsync();
        if (customerProfile == null)
        {
            return ApiResponse<ReviewResponse>.Fail("Customer profile not found.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .FirstOrDefaultAsync(b => b.Id == request.BookingId);

        if (booking == null)
        {
            return ApiResponse<ReviewResponse>.Fail("Booking not found.");
        }

        if (booking.CustomerId != customerProfile.Id)
        {
            return ApiResponse<ReviewResponse>.Fail("You do not own this booking.");
        }

        if (booking.Status != (int)BookingStatus.CompletedPendingSettlement && booking.Status != (int)BookingStatus.Settled)
        {
            return ApiResponse<ReviewResponse>.Fail("Reviews can only be created for completed or settled bookings.");
        }

        var existingReview = await _unitOfWork.Reviews.Query()
            .AnyAsync(r => r.BookingId == booking.Id);

        if (existingReview)
        {
            return ApiResponse<ReviewResponse>.Fail("Booking already has a review.");
        }

        request.Comment = request.Comment?.Trim();

        if (request.Rating < 1 || request.Rating > 5)
        {
            return ApiResponse<ReviewResponse>.Fail("Rating must be between 1 and 5.");
        }

        if (request.Comment != null && request.Comment.Length > 500)
        {
            return ApiResponse<ReviewResponse>.Fail("Comment cannot exceed 500 characters.");
        }

        var review = new Review
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            CustomerId = booking.CustomerId,
            PtProfileId = booking.PtProfileId,
            Rating = request.Rating,
            Comment = request.Comment, // Delimiter: initially only has the customer comment
            IsHidden = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Reviews.AddAsync(review);

        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = _currentUserService.UserId!.Value,
                Action = "CreateReview",
                EntityName = "Review",
                EntityId = review.Id,
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { BookingId = booking.Id, Rating = review.Rating }),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
        }
        catch
        {
            // silently ignore
        }

        await _unitOfWork.SaveChangesAsync();

        await RecalculatePtRatingAsync(booking.PtProfileId);

        var response = MapToReviewResponse(review);
        return ApiResponse<ReviewResponse>.Ok(response, "Review created successfully.");
    }

    public async Task<ApiResponse<ReviewResponse>> UpdateReviewAsync(Guid reviewId, UpdateReviewRequest request)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<ReviewResponse>.Fail("User is not authenticated.");
        }

        if (_currentUserService.Role != (int)UserRole.Customer)
        {
            return ApiResponse<ReviewResponse>.Fail("Only customers can update reviews.");
        }

        var customerProfile = await GetCurrentCustomerProfileAsync();
        if (customerProfile == null)
        {
            return ApiResponse<ReviewResponse>.Fail("Customer profile not found.");
        }

        var review = await _unitOfWork.Reviews.Query()
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
        {
            return ApiResponse<ReviewResponse>.Fail("Review not found.");
        }

        if (review.CustomerId != customerProfile.Id)
        {
            return ApiResponse<ReviewResponse>.Fail("You do not own this review.");
        }

        if ((DateTime.UtcNow - review.CreatedAt).TotalDays > 7)
        {
            return ApiResponse<ReviewResponse>.Fail("Reviews can only be updated within 7 days after creation.");
        }

        request.Comment = request.Comment?.Trim();

        if (request.Rating < 1 || request.Rating > 5)
        {
            return ApiResponse<ReviewResponse>.Fail("Rating must be between 1 and 5.");
        }

        if (request.Comment != null && request.Comment.Length > 500)
        {
            return ApiResponse<ReviewResponse>.Fail("Comment cannot exceed 500 characters.");
        }

        // Preserve PT reply if it exists under the delimiter separation hack
        var parts = review.Comment?.Split("|||", 2);
        string? existingReply = parts?.Length > 1 ? parts[1] : null;

        review.Rating = request.Rating;
        review.Comment = existingReply != null ? $"{request.Comment}|||{existingReply}" : request.Comment;
        review.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Reviews.Update(review);
        await _unitOfWork.SaveChangesAsync();

        await RecalculatePtRatingAsync(review.PtProfileId);

        var response = MapToReviewResponse(review);
        return ApiResponse<ReviewResponse>.Ok(response, "Review updated successfully.");
    }

    public async Task<ApiResponse<ReviewResponse>> ReplyReviewAsync(Guid reviewId, string reply)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<ReviewResponse>.Fail("User is not authenticated.");
        }

        if (_currentUserService.Role != (int)UserRole.PersonalTrainer && _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<ReviewResponse>.Fail("Only personal trainers or admins can reply to reviews.");
        }

        var review = await _unitOfWork.Reviews.Query()
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
        {
            return ApiResponse<ReviewResponse>.Fail("Review not found.");
        }

        if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await GetCurrentPtProfileAsync();
            if (ptProfile == null || review.PtProfileId != ptProfile.Id)
            {
                return ApiResponse<ReviewResponse>.Fail("You can only reply to reviews for your own profile.");
            }
        }

        reply = reply?.Trim()!;
        if (string.IsNullOrWhiteSpace(reply))
        {
            return ApiResponse<ReviewResponse>.Fail("Reply content is required.");
        }
        if (reply.Length > 500)
        {
            return ApiResponse<ReviewResponse>.Fail("Reply cannot exceed 500 characters.");
        }

        // Update PT reply using the delimiter separation hack
        var parts = review.Comment?.Split("|||", 2);
        string customerComment = parts?.Length > 0 ? parts[0] : string.Empty;
        review.Comment = $"{customerComment}|||{reply}";
        review.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Reviews.Update(review);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToReviewResponse(review);
        return ApiResponse<ReviewResponse>.Ok(response, "Reply submitted successfully.");
    }

    public async Task<ApiResponse<bool>> HideReviewAsync(Guid reviewId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<bool>.Fail("User is not authenticated.");
        }

        if (_currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<bool>.Fail("Only admins can hide reviews.");
        }

        var review = await _unitOfWork.Reviews.Query()
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
        {
            return ApiResponse<bool>.Fail("Review not found.");
        }

        review.IsHidden = true;
        review.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Reviews.Update(review);
        await _unitOfWork.SaveChangesAsync();

        await RecalculatePtRatingAsync(review.PtProfileId);

        return ApiResponse<bool>.Ok(true, "Review hidden successfully.");
    }

    public async Task<ApiResponse<ReviewResponse>> GetReviewByIdAsync(Guid reviewId)
    {
        var review = await _unitOfWork.Reviews.Query()
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
        {
            return ApiResponse<ReviewResponse>.Fail("Review not found.");
        }

        if (review.IsHidden)
        {
            bool isAdmin = _currentUserService.IsAuthenticated && _currentUserService.Role == (int)UserRole.Admin;
            if (!isAdmin)
            {
                return ApiResponse<ReviewResponse>.Fail("Review not found.");
            }
        }

        var response = MapToReviewResponse(review);
        return ApiResponse<ReviewResponse>.Ok(response, "Review retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<ReviewResponse>>> GetReviewsByPtAsync(Guid ptProfileId)
    {
        var reviews = await _unitOfWork.Reviews.Query()
            .Where(r => r.PtProfileId == ptProfileId && !r.IsHidden)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var response = reviews.Select(MapToReviewResponse).ToList();
        return ApiResponse<IReadOnlyList<ReviewResponse>>.Ok(response, "Reviews retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<ReviewResponse>>> GetMyReviewsAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<IReadOnlyList<ReviewResponse>>.Fail("User is not authenticated.");
        }

        List<Review> reviews;
        if (_currentUserService.Role == (int)UserRole.Customer)
        {
            var customerProfile = await GetCurrentCustomerProfileAsync();
            if (customerProfile == null)
            {
                return ApiResponse<IReadOnlyList<ReviewResponse>>.Fail("Customer profile not found.");
            }
            reviews = await _unitOfWork.Reviews.Query()
                .Where(r => r.CustomerId == customerProfile.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        else if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await GetCurrentPtProfileAsync();
            if (ptProfile == null)
            {
                return ApiResponse<IReadOnlyList<ReviewResponse>>.Fail("Personal trainer profile not found.");
            }
            reviews = await _unitOfWork.Reviews.Query()
                .Where(r => r.PtProfileId == ptProfile.Id && !r.IsHidden)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        else
        {
            // Admin can see all reviews
            reviews = await _unitOfWork.Reviews.Query()
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        var response = reviews.Select(MapToReviewResponse).ToList();
        return ApiResponse<IReadOnlyList<ReviewResponse>>.Ok(response, "Reviews retrieved successfully.");
    }

    public async Task<ApiResponse<string>> DeleteReviewAsync(Guid reviewId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<string>.Fail("User is not authenticated.");
        }

        var review = await _unitOfWork.Reviews.Query()
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
        {
            return ApiResponse<string>.Fail("Review not found.");
        }

        bool canDelete = false;
        if (_currentUserService.Role == (int)UserRole.Admin)
        {
            canDelete = true;
        }
        else if (_currentUserService.Role == (int)UserRole.Customer)
        {
            var customer = await GetCurrentCustomerProfileAsync();
            if (customer != null && review.CustomerId == customer.Id)
            {
                canDelete = true;
            }
        }

        if (!canDelete)
        {
            return ApiResponse<string>.Fail("Access denied.");
        }

        _unitOfWork.Reviews.Delete(review);
        await _unitOfWork.SaveChangesAsync();

        await RecalculatePtRatingAsync(review.PtProfileId);

        return ApiResponse<string>.Ok(reviewId.ToString(), "Review deleted successfully.");
    }

    #region Helper Methods

    private async Task RecalculatePtRatingAsync(Guid ptProfileId)
    {
        var ptProfile = await _unitOfWork.PtProfiles.Query()
            .FirstOrDefaultAsync(p => p.Id == ptProfileId);

        if (ptProfile != null)
        {
            var activeReviews = await _unitOfWork.Reviews.Query()
                .Where(r => r.PtProfileId == ptProfileId && !r.IsHidden)
                .ToListAsync();

            if (activeReviews.Any())
            {
                ptProfile.AverageRating = (decimal)activeReviews.Average(r => r.Rating);
                ptProfile.TotalReviews = activeReviews.Count;
            }
            else
            {
                ptProfile.AverageRating = 0m;
                ptProfile.TotalReviews = 0;
            }

            ptProfile.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PtProfiles.Update(ptProfile);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    private async Task<CustomerProfile?> GetCurrentCustomerProfileAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return null;
        }

        var userId = _currentUserService.UserId.Value;
        return await _unitOfWork.CustomerProfiles.Query()
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
    }

    private async Task<PtProfile?> GetCurrentPtProfileAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return null;
        }

        var userId = _currentUserService.UserId.Value;
        return await _unitOfWork.PtProfiles.Query()
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
    }

    private ReviewResponse MapToReviewResponse(Review review)
    {
        var parts = review.Comment?.Split("|||", 2);
        string? customerComment = parts?.Length > 0 ? parts[0] : null;
        string? ptReply = parts?.Length > 1 ? parts[1] : null;

        return new ReviewResponse
        {
            Id = review.Id,
            BookingId = review.BookingId,
            CustomerId = review.CustomerId,
            PtProfileId = review.PtProfileId,
            Rating = review.Rating,
            Comment = customerComment,
            PtReply = ptReply,
            IsHidden = review.IsHidden,
            CreatedAt = review.CreatedAt
        };
    }

    #endregion
}
