using System;
using System.Collections.Generic;
using LockedIn.BusinessObject.DTOs.PtProfiles;

namespace LockedIn.BusinessObject.DTOs.Admin;

public class PtVerificationDetailResponse
{
    public PtProfileResponse Profile { get; set; } = null!;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public IReadOnlyList<PtDocumentResponse> Documents { get; set; } = new List<PtDocumentResponse>();
}
