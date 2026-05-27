using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class Payment
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public string Provider { get; set; } = null!;

    public string OrderCode { get; set; } = null!;

    public decimal Amount { get; set; }

    public int Status { get; set; }

    public string? CheckoutUrl { get; set; }

    public string? ProviderTransactionId { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual ICollection<PaymentWebhookLog> PaymentWebhookLogs { get; set; } = new List<PaymentWebhookLog>();
}
