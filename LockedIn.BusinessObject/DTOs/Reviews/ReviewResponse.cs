using System;

namespace LockedIn.BusinessObject.DTOs.Reviews;

public class ReviewResponse
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid PtProfileId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string? PtReply { get; set; }
    public bool IsHidden { get; set; }
    public DateTime CreatedAt { get; set; }
}
