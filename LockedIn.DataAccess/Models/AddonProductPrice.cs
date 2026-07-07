using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class AddonProductPrice
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public decimal UnitAmount { get; set; }
    public string Currency { get; set; } = null!;
    public bool IsActive { get; set; }
    public Guid? CreatedByAdminId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }

    public virtual AddonProduct Product { get; set; } = null!;
    public virtual User? CreatedByAdmin { get; set; }
    public virtual ICollection<AddonOrderItem> AddonOrderItems { get; set; } = new List<AddonOrderItem>();
}
