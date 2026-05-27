using System;

namespace LockedIn.BusinessObject.DTOs.Payments;

public class PayOsWebhookRequest
{
    public string Code { get; set; } = null!;
    public string Desc { get; set; } = null!;
    public string Data { get; set; } = null!;
    public string Signature { get; set; } = null!;
}
