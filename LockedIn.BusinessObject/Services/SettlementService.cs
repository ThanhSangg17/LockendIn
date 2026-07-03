using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.DTOs.Settlements;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Services;

public class SettlementService : ISettlementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SettlementService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<SettlementHistoryResult>> GetMySettlementsAsync(
        PaginationRequest request, 
        int? settlementStatus, 
        DateTime? startDate, 
        DateTime? endDate)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<SettlementHistoryResult>.Fail("User is not authenticated.");
        }

        if (_currentUserService.Role == (int)UserRole.Customer)
        {
            return ApiResponse<SettlementHistoryResult>.Fail("Only PT or Admin can view settlements.");
        }

        IQueryable<Settlement> query = _unitOfWork.Settlements.Query()
            .Include(s => s.Booking)
            .ThenInclude(b => b.Package)
            .Include(s => s.Booking)
            .ThenInclude(b => b.Customer)
            .ThenInclude(c => c.User);

        if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await GetCurrentPtProfileAsync();
            if (ptProfile == null)
            {
                return ApiResponse<SettlementHistoryResult>.Fail("Personal trainer profile not found.");
            }
            query = query.Where(s => s.PtProfileId == ptProfile.Id);
        }
        else if (_currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<SettlementHistoryResult>.Fail("Access denied.");
        }

        if (settlementStatus.HasValue)
        {
            query = query.Where(s => s.Status == settlementStatus.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(s => s.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(s => s.CreatedAt <= endDate.Value);
        }

        // Calculate totals based on the filtered query
        var totalSettled = await query.Where(s => s.Status == (int)SettlementStatus.Settled).SumAsync(s => s.NetAmount);
        var totalPending = await query.Where(s => s.Status == (int)SettlementStatus.Pending).SumAsync(s => s.NetAmount);
        var countSettled = await query.Where(s => s.Status == (int)SettlementStatus.Settled).CountAsync();
        var countPending = await query.Where(s => s.Status == (int)SettlementStatus.Pending).CountAsync();

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var settlements = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var items = settlements.Select(s => new SettlementHistoryResponse
        {
            Id = s.Id,
            BookingId = s.BookingId,
            PtProfileId = s.PtProfileId,
            GrossAmount = s.GrossAmount,
            PlatformFee = s.PlatformFee,
            NetAmount = s.NetAmount,
            Status = s.Status,
            SettledAt = s.SettledAt,
            CreatedAt = s.CreatedAt,
            PackageName = s.Booking?.Package?.Name ?? string.Empty,
            CustomerName = s.Booking?.Customer?.User?.FullName ?? string.Empty
        }).ToList();

        var pagedResult = new PagedResult<SettlementHistoryResponse>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };

        var historyResult = new SettlementHistoryResult
        {
            TotalSettled = totalSettled,
            TotalPending = totalPending,
            CountSettled = countSettled,
            CountPending = countPending,
            Settlements = pagedResult
        };

        return ApiResponse<SettlementHistoryResult>.Ok(historyResult, "Settlements retrieved successfully.");
    }

    public async Task<ApiResponse<SettlementResponse>> GetSettlementByIdAsync(Guid settlementId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<SettlementResponse>.Fail("User is not authenticated.");
        }

        if (_currentUserService.Role == (int)UserRole.Customer)
        {
            return ApiResponse<SettlementResponse>.Fail("Customers are not allowed to view settlements.");
        }

        var settlement = await _unitOfWork.Settlements.Query()
            .FirstOrDefaultAsync(s => s.Id == settlementId);

        if (settlement == null)
        {
            return ApiResponse<SettlementResponse>.Fail("Settlement not found.");
        }

        if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await GetCurrentPtProfileAsync();
            if (ptProfile == null || settlement.PtProfileId != ptProfile.Id)
            {
                return ApiResponse<SettlementResponse>.Fail("Access denied to this settlement.");
            }
        }
        else if (_currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<SettlementResponse>.Fail("Access denied.");
        }

        var response = MapToSettlementResponse(settlement);
        return ApiResponse<SettlementResponse>.Ok(response, "Settlement details retrieved successfully.");
    }

    #region Helper Methods

    private async Task<PtProfile?> GetCurrentPtProfileAsync()
    {
        var userId = _currentUserService.UserId!.Value;
        return await _unitOfWork.PtProfiles.Query()
            .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);
    }

    private SettlementResponse MapToSettlementResponse(Settlement settlement)
    {
        return new SettlementResponse
        {
            Id = settlement.Id,
            BookingId = settlement.BookingId,
            PtProfileId = settlement.PtProfileId,
            GrossAmount = settlement.GrossAmount,
            PlatformFee = settlement.PlatformFee,
            NetAmount = settlement.NetAmount,
            Status = settlement.Status,
            SettledAt = settlement.SettledAt,
            CreatedAt = settlement.CreatedAt
        };
    }

    #endregion
}
