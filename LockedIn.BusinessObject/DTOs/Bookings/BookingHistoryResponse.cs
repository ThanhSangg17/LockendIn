using System;

namespace LockedIn.BusinessObject.DTOs.Bookings;

public class BookingHistoryResponse : BookingResponse
{
    public string PackageName { get; set; } = string.Empty;
    public string PtName { get; set; } = string.Empty;
    public int? PaymentStatus { get; set; }
    public DateTime? PaidAt { get; set; }
}
