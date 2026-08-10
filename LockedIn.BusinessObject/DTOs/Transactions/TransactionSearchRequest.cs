using System;
using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.DTOs.Transactions;

public class TransactionSearchRequest : PaginationRequest
{
    public string? TransactionType { get; set; } // "PAYMENT" | "SETTLEMENT"
    public int? Status { get; set; }
    public string? Search { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
