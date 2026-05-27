using System;

namespace LockedIn.BusinessObject.DTOs.Marketplace;

public class MarketplacePtDetailResponse
{
    public Guid PtProfileId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? Specialization { get; set; }
    public int ExperienceYears { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
}
