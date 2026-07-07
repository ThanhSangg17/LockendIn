using System;
using System.Collections.Generic;

namespace LockedIn.BusinessObject.DTOs.Admin;

public class AddonProductResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int ProductType { get; set; }
    public int? GrantQuantity { get; set; }
    public int? DurationDays { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<AddonProductPriceResponse> Prices { get; set; } = new();
}

public class AddonProductPriceResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public decimal UnitAmount { get; set; }
    public string Currency { get; set; } = null!;
    public bool IsActive { get; set; }
    public Guid? CreatedByAdminId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }
}
