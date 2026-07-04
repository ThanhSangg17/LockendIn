using System;

namespace LockedIn.BusinessObject.DTOs.PtProfile;

public class ProfileEditRequestResponse
{
    public Guid Id { get; set; }
    public Guid PtProfileId { get; set; }
    public string? CurrentBio { get; set; }
    public string? CurrentSpecialization { get; set; }
    public int CurrentExperienceYears { get; set; }
    public string? RequestedBio { get; set; }
    public string? RequestedSpecialization { get; set; }
    public int RequestedExperienceYears { get; set; }
    public int Status { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedByAdminId { get; set; }
}
