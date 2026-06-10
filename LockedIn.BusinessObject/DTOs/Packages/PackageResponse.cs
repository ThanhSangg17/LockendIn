using System;

namespace LockedIn.BusinessObject.DTOs.Packages;

public class PackageResponse
{
    public Guid Id { get; set; }
    public Guid PtProfileId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int SessionCount { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}
