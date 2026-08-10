using System;

namespace LockedIn.BusinessObject.DTOs.Transactions;

public class UserTransactionResponse
{
    public Guid Id { get; set; }
    public string TransactionType { get; set; } = null!; // "PAYMENT" | "SETTLEMENT"
    public string ReferenceCode { get; set; } = null!;
    public Guid BookingId { get; set; }
    public string PackageName { get; set; } = null!;
    
    // Counterparty Info
    public string CounterpartyName { get; set; } = null!;
    
    public decimal Amount { get; set; }
    public decimal? GrossAmount { get; set; }
    public decimal? PlatformFee { get; set; }
    
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
