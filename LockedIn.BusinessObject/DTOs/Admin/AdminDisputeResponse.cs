using System;

namespace LockedIn.BusinessObject.DTOs.Admin;

public class AdminDisputeResponse
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid PtProfileId { get; set; }
    public string Reason { get; set; } = null!;
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
