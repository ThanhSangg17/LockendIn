using System;

namespace LockedIn.BusinessObject.DTOs.Bookings;

public class BookingResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid PtProfileId { get; set; }
    public Guid PackageId { get; set; }
    public int Status { get; set; }
    public decimal TotalAmount { get; set; }
    public int SessionCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
