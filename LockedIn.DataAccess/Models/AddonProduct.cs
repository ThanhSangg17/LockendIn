using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class AddonProduct
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

    public virtual ICollection<AddonProductPrice> AddonProductPrices { get; set; } = new List<AddonProductPrice>();
    public virtual ICollection<AddonOrderItem> AddonOrderItems { get; set; } = new List<AddonOrderItem>();
}
