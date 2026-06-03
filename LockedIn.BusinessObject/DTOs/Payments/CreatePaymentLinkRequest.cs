using System;
using System.ComponentModel.DataAnnotations;

namespace LockedIn.BusinessObject.DTOs.Payments;

public class CreatePaymentLinkRequest
{
    [Required(ErrorMessage = "Booking ID is required.")]
    public Guid BookingId { get; set; }
}
