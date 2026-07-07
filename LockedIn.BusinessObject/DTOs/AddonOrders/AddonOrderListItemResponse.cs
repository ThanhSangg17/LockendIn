using System;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.DTOs.AddonOrders;

public class AddonOrderListItemResponse
{
    public Guid OrderId { get; set; }
    public AddonOrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string ProductCode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public AddonPaymentAttemptStatus LatestPaymentAttemptStatus { get; set; }
    public string? LatestCheckoutUrl { get; set; }
    public DateTime? LatestExpiredAt { get; set; }
}
