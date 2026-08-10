using System;

namespace LockedIn.BusinessObject.DTOs.Transactions;

public class AdminTransactionResponse
{
    public Guid Id { get; set; }
    public string TransactionType { get; set; } = null!; // "PAYMENT" | "SETTLEMENT"
    public string ReferenceCode { get; set; } = null!;   // OrderCode hoặc Settlement Id prefix
    public Guid BookingId { get; set; }
    public string PackageName { get; set; } = null!;
    
    // Customer Info
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    
    // PT Info
    public Guid PtProfileId { get; set; }
    public string PtName { get; set; } = null!;
    public string PtEmail { get; set; } = null!;
    
    // Financial Amounts
    public decimal GrossAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal NetAmount { get; set; }
    
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
