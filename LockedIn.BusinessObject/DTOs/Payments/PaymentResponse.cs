using System;

namespace LockedIn.BusinessObject.DTOs.Payments;

public class PaymentResponse
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string Provider { get; set; } = null!;
    public string OrderCode { get; set; } = null!;
    public decimal Amount { get; set; }
    public int Status { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? ProviderTransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
