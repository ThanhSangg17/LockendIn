using System;

namespace LockedIn.BusinessObject.DTOs.Admin;

public class CreateAddonProductPriceRequest
{
    public decimal UnitAmount { get; set; }
    public string Currency { get; set; } = "VND";
}
