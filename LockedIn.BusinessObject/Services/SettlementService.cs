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

    public async Task<ApiResponse<IReadOnlyList<SettlementResponse>>> GetMySettlementsAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<IReadOnlyList<SettlementResponse>>.Fail("User is not authenticated.");
        }

        if (_currentUserService.Role == (int)UserRole.Customer)
        {
            return ApiResponse<IReadOnlyList<SettlementResponse>>.Fail("Only PT or Admin can view settlements.");
        }

        IQueryable<Settlement> query = _unitOfWork.Settlements.Query();

        if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await GetCurrentPtProfileAsync();
            if (ptProfile == null)
            {
                return ApiResponse<IReadOnlyList<SettlementResponse>>.Fail("Personal trainer profile not found.");
            }
            query = query.Where(s => s.PtProfileId == ptProfile.Id);
        }
        else if (_currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<IReadOnlyList<SettlementResponse>>.Fail("Access denied.");
        }

        var settlements = await query
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var response = settlements.Select(MapToSettlementResponse).ToList();
        return ApiResponse<IReadOnlyList<SettlementResponse>>.Ok(response, "Settlements retrieved successfully.");
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
