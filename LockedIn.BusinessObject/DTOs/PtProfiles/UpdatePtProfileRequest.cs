using System;

namespace LockedIn.BusinessObject.DTOs.PtProfiles;

public class UpdatePtProfileRequest
{
    public string? Bio { get; set; }
    public string? Specialization { get; set; }
    public int ExperienceYears { get; set; }
}
