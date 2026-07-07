using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class AddonPaymentAttempt
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Provider { get; set; } = null!;
    public string OrderCode { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public int Status { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? ProviderTransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual AddonOrder Order { get; set; } = null!;
    public virtual ICollection<AddonWebhookLog> AddonWebhookLogs { get; set; } = new List<AddonWebhookLog>();
}
