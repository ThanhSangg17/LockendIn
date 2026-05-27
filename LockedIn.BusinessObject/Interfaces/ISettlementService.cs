using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.Interfaces;

public interface ISettlementService
{
    Task<ApiResponse<string>> GetMySettlementsAsync();
    Task<ApiResponse<string>> GetSettlementByIdAsync(Guid settlementId);
}
