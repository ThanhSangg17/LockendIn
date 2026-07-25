using System;

namespace LockedIn.BusinessObject.DTOs.Admin;

public class AdminDisputeResponse
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

    // Enhanced context details for Admin review
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? PtName { get; set; }
    public string? PtEmail { get; set; }

    public string? PackageName { get; set; }
    public decimal? BookingAmount { get; set; }
    public int? BookingStatus { get; set; }
    public DateTime? BookingStartDate { get; set; }
    public DateTime? BookingCompletedAt { get; set; }

    public System.Collections.Generic.List<LockedIn.BusinessObject.DTOs.Disputes.DisputeEvidenceResponse> Evidences { get; set; } = new();
}
