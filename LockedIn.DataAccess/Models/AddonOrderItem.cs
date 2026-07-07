using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class AddonOrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public Guid PriceId { get; set; }
    public string ProductCode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public decimal UnitAmount { get; set; }
    public string Currency { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string FulfillmentTypeSnapshot { get; set; } = null!;
    public int? GrantQuantitySnapshot { get; set; }
    public int? DurationDaysSnapshot { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual AddonOrder Order { get; set; } = null!;
    public virtual AddonProduct Product { get; set; } = null!;
    public virtual AddonProductPrice Price { get; set; } = null!;
    public virtual ICollection<AddonEntitlement> AddonEntitlements { get; set; } = new List<AddonEntitlement>();
}
