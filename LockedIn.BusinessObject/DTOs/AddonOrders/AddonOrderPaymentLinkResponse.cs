using System;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.DTOs.AddonOrders;

public class AddonOrderPaymentLinkResponse
{
    public Guid OrderId { get; set; }
    public AddonOrderStatus OrderStatus { get; set; }
    public string ProductCode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public decimal UnitAmount { get; set; }
    public string Currency { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid PaymentAttemptId { get; set; }
    public AddonPaymentAttemptStatus PaymentAttemptStatus { get; set; }
    public string OrderCode { get; set; } = null!;
    public string? CheckoutUrl { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
