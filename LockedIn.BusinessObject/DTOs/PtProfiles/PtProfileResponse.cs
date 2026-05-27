using System;

namespace LockedIn.BusinessObject.DTOs.PtProfiles;

public class PtProfileResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Bio { get; set; }
    public string? Specialization { get; set; }
    public int ExperienceYears { get; set; }
    public int VerificationStatus { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
}
