using System;

namespace LockedIn.BusinessObject.DTOs.Disputes;

public class UploadDisputeEvidenceRequest
{
    public string FileUrl { get; set; } = null!;
    public string? FileType { get; set; }
}
