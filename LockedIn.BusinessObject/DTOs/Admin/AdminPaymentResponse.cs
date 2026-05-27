using System;

namespace LockedIn.BusinessObject.DTOs.Admin;

public class AdminPaymentResponse
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string Provider { get; set; } = null!;
    public string OrderCode { get; set; } = null!;
    public decimal Amount { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
