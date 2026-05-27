using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.DTOs.Settlements;

namespace LockedIn.BusinessObject.Services;

public class SettlementService : ISettlementService
{
    private readonly IUnitOfWork _unitOfWork;

    public SettlementService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<IReadOnlyList<SettlementResponse>>> GetMySettlementsAsync()
    {
        return await Task.FromResult(ApiResponse<IReadOnlyList<SettlementResponse>>.Ok(new List<SettlementResponse>(), "Not implemented yet"));
    }

    public async Task<ApiResponse<SettlementResponse>> GetSettlementByIdAsync(Guid settlementId)
    {
        return await Task.FromResult(ApiResponse<SettlementResponse>.Ok(new SettlementResponse(), "Not implemented yet"));
    }
}
