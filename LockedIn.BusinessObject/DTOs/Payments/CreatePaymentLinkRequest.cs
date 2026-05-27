using System;

namespace LockedIn.BusinessObject.DTOs.Payments;

public class CreatePaymentLinkRequest
{
    public Guid BookingId { get; set; }
}
