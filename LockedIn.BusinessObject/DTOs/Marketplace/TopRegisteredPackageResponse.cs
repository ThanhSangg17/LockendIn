using System;

namespace LockedIn.BusinessObject.DTOs.Marketplace;

public class TopRegisteredPackageResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int SessionCount { get; set; }
    public Guid PtProfileId { get; set; }
    public string PtName { get; set; } = null!;
    public string? PtAvatarUrl { get; set; }
    public int RegisteredUserCount { get; set; }
}
