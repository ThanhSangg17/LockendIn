using System;

namespace LockedIn.BusinessObject.DTOs.PtProfiles;

public class UploadPtDocumentRequest
{
    public int DocumentType { get; set; }
    public string FileUrl { get; set; } = null!;
}
