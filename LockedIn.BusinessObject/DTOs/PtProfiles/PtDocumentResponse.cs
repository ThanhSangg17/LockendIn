using System;

namespace LockedIn.BusinessObject.DTOs.PtProfiles;

public class PtDocumentResponse
{
    public Guid Id { get; set; }
    public Guid PtProfileId { get; set; }
    public int DocumentType { get; set; }
    public string FileUrl { get; set; } = null!;
    public int Status { get; set; }
    public DateTime UploadedAt { get; set; }
}
