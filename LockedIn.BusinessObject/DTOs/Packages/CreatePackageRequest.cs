using System;

namespace LockedIn.BusinessObject.DTOs.Packages;

public class CreatePackageRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int SessionCount { get; set; }
    public decimal Price { get; set; }
}
