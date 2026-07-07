using System;

namespace LockedIn.BusinessObject.DTOs.Admin;

public class CreateAddonProductRequest
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int ProductType { get; set; }
    public int? GrantQuantity { get; set; }
    public int? DurationDays { get; set; }
    public bool IsActive { get; set; } = true;
}
