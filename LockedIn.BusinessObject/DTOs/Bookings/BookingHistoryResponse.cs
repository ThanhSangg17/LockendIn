using System;

namespace LockedIn.BusinessObject.DTOs.Bookings;

public class BookingHistoryResponse : BookingResponse
{
    public int? PaymentStatus { get; set; }
    public DateTime? PaidAt { get; set; }
}
