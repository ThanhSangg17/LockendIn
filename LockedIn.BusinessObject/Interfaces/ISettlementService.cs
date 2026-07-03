using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Settlements;

namespace LockedIn.BusinessObject.Interfaces;

public interface ISettlementService
{
    Task<ApiResponse<SettlementHistoryResult>> GetMySettlementsAsync(PaginationRequest request, int? settlementStatus, DateTime? startDate, DateTime? endDate);
    Task<ApiResponse<SettlementResponse>> GetSettlementByIdAsync(Guid settlementId);
}
