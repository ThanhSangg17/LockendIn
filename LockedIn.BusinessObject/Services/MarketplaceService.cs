using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.DTOs.Marketplace;
using LockedIn.BusinessObject.DTOs.Packages;
using LockedIn.BusinessObject.DTOs.Reviews;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Services;

public class MarketplaceService : IMarketplaceService
{
    private readonly IUnitOfWork _unitOfWork;

    public MarketplaceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<MarketplacePtResponse>>> GetPtsAsync(PtSearchRequest request)
    {
        var query = _unitOfWork.PtProfiles.Query()
            .Include(pt => pt.User)
            .Where(pt => !pt.IsDeleted && 
                         !pt.User.IsDeleted && 
                         pt.User.Status == 1 && 
                         pt.VerificationStatus == (int)PtVerificationStatus.Approved);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            request.Keyword = request.Keyword.Trim();
            var keyword = request.Keyword.ToLower();
            query = query.Where(pt => 
                pt.User.FullName.ToLower().Contains(keyword) || 
                (pt.Specialization != null && pt.Specialization.ToLower().Contains(keyword)) ||
                (pt.Bio != null && pt.Bio.ToLower().Contains(keyword))
            );
        }

        if (request.MinRating.HasValue)
        {
            query = query.Where(pt => pt.AverageRating >= request.MinRating.Value);
        }

        var totalCount = await query.CountAsync();

        var pageNumber = request.PageNumber;
        var pageSize = request.PageSize;

        var pts = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = pts.Select(pt => new MarketplacePtResponse
        {
            PtProfileId = pt.Id,
            FullName = pt.User.FullName,
            AvatarUrl = pt.User.AvatarUrl,
            Specialization = pt.Specialization,
            ExperienceYears = pt.ExperienceYears,
            AverageRating = pt.AverageRating,
            TotalReviews = pt.TotalReviews
        }).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var result = new PagedResult<MarketplacePtResponse>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = totalPages
        };

        return ApiResponse<PagedResult<MarketplacePtResponse>>.Ok(result, "Personal trainers retrieved successfully.");
    }

    public async Task<ApiResponse<MarketplacePtDetailResponse>> GetPtDetailAsync(Guid ptProfileId)
    {
        var ptProfile = await _unitOfWork.PtProfiles.Query()
            .Include(pt => pt.User)
            .FirstOrDefaultAsync(pt => pt.Id == ptProfileId && 
                                       !pt.IsDeleted && 
                                       !pt.User.IsDeleted && 
                                       pt.User.Status == 1 && 
                                       pt.VerificationStatus == (int)PtVerificationStatus.Approved);

        if (ptProfile == null)
        {
            return ApiResponse<MarketplacePtDetailResponse>.Fail("PT profile not found");
        }

        var detail = new MarketplacePtDetailResponse
        {
            PtProfileId = ptProfile.Id,
            UserId = ptProfile.UserId,
            FullName = ptProfile.User.FullName,
            AvatarUrl = ptProfile.User.AvatarUrl,
            Bio = ptProfile.Bio,
            Specialization = ptProfile.Specialization,
            ExperienceYears = ptProfile.ExperienceYears,
            AverageRating = ptProfile.AverageRating,
            TotalReviews = ptProfile.TotalReviews
        };

        return ApiResponse<MarketplacePtDetailResponse>.Ok(detail, "PT profile details retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<PackageResponse>>> GetPtPackagesAsync(Guid ptProfileId)
    {
        var ptExists = await _unitOfWork.PtProfiles.Query()
            .Include(pt => pt.User)
            .AnyAsync(pt => pt.Id == ptProfileId && 
                             !pt.IsDeleted && 
                             !pt.User.IsDeleted && 
                             pt.User.Status == 1 && 
                             pt.VerificationStatus == (int)PtVerificationStatus.Approved);

        if (!ptExists)
        {
            return ApiResponse<IReadOnlyList<PackageResponse>>.Fail("PT profile not found or not approved.");
        }

        var packages = await _unitOfWork.Packages.Query()
            .Where(p => p.PtProfileId == ptProfileId && p.IsActive && !p.IsDeleted)
            .ToListAsync();

        var response = packages.Select(p => new PackageResponse
        {
            Id = p.Id,
            PtProfileId = p.PtProfileId,
            Name = p.Name,
            Description = p.Description,
            SessionCount = p.SessionCount,
            Price = p.Price,
            IsActive = p.IsActive
        }).ToList();

        return ApiResponse<IReadOnlyList<PackageResponse>>.Ok(response, "Packages retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<ReviewResponse>>> GetPtReviewsAsync(Guid ptProfileId)
    {
        var reviews = await _unitOfWork.Reviews.Query()
            .Where(r => r.PtProfileId == ptProfileId && !r.IsHidden)
            .ToListAsync();

        var response = reviews.Select(r => {
            var parts = r.Comment?.Split("|||", 2);
            return new ReviewResponse
            {
                Id = r.Id,
                BookingId = r.BookingId,
                CustomerId = r.CustomerId,
                PtProfileId = r.PtProfileId,
                Rating = r.Rating,
                Comment = parts?.Length > 0 ? parts[0] : null,
                PtReply = parts?.Length > 1 ? parts[1] : null,
                IsHidden = r.IsHidden,
                CreatedAt = r.CreatedAt
            };
        }).ToList();

        return ApiResponse<IReadOnlyList<ReviewResponse>>.Ok(response, "Reviews retrieved successfully.");
    }
}

