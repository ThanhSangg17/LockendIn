using System;

namespace LockedIn.BusinessObject.DTOs.Settlements;

public class SettlementHistoryResponse : SettlementResponse
{
    public string PackageName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
}
