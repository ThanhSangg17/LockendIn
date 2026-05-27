using System;

namespace LockedIn.BusinessObject.DTOs.Bookings;

public class BookingDetailResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid PtProfileId { get; set; }
    public Guid PackageId { get; set; }
    public int Status { get; set; }
    public decimal TotalAmount { get; set; }
    public int SessionCount { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? SettlementDueAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
