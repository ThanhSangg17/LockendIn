using System;

namespace LockedIn.BusinessObject.DTOs.Customers;

public class CustomerProfileResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public string? FitnessGoal { get; set; }
    public string? HealthNote { get; set; }
}
