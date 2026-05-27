using System;

namespace LockedIn.BusinessObject.DTOs.Disputes;

public class DisputeEvidenceResponse
{
    public Guid Id { get; set; }
    public Guid DisputeId { get; set; }
    public string FileUrl { get; set; } = null!;
    public string? FileType { get; set; }
    public DateTime UploadedAt { get; set; }
}
