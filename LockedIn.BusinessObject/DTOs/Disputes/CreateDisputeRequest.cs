using System;

namespace LockedIn.BusinessObject.DTOs.Disputes;

public class CreateDisputeRequest
{
    public Guid BookingId { get; set; }
    public string Reason { get; set; } = null!;
    public string Description { get; set; } = null!;
}
