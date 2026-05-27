using System;

namespace LockedIn.BusinessObject.DTOs.Admin;

public class AdminSettlementResponse
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid PtProfileId { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal NetAmount { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
