using System;

namespace LockedIn.BusinessObject.DTOs.Customers;

public class UpdateCustomerProfileRequest
{
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public string? FitnessGoal { get; set; }
    public string? HealthNote { get; set; }
}
