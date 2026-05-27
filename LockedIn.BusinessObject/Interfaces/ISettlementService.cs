using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Settlements;

namespace LockedIn.BusinessObject.Interfaces;

public interface ISettlementService
{
    Task<ApiResponse<IReadOnlyList<SettlementResponse>>> GetMySettlementsAsync();
    Task<ApiResponse<SettlementResponse>> GetSettlementByIdAsync(Guid settlementId);
}
