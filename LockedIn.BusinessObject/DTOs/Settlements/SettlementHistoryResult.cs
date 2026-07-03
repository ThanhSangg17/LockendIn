using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.DTOs.Settlements;

public class SettlementHistoryResult
{
    public decimal TotalSettled { get; set; }
    public decimal TotalPending { get; set; }
    public int CountSettled { get; set; }
    public int CountPending { get; set; }
    public PagedResult<SettlementHistoryResponse> Settlements { get; set; } = null!;
}
