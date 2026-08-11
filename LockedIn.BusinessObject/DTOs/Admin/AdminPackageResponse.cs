using System;

namespace LockedIn.BusinessObject.DTOs.Admin;

public class AdminPackageResponse
{
    public Guid Id { get; set; }
    public Guid PtProfileId { get; set; }
    public string PtFullName { get; set; } = string.Empty;
    public string PtEmail { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SessionCount { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
