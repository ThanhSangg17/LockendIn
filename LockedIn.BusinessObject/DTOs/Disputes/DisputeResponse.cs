using System;

namespace LockedIn.BusinessObject.DTOs.Disputes;

public class DisputeResponse
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid PtProfileId { get; set; }
    public string Reason { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Status { get; set; }
    public string? ResolutionNote { get; set; }
    public Guid? ResolvedByAdminId { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
