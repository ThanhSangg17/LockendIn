using System;
using System.Collections.Generic;

namespace LockedIn.BusinessObject.DTOs.Admin;

public class DashboardAnalyticsResponse
{
    public DashboardKpiDto Kpis { get; set; } = null!;
    public BookingStatusSummaryDto BookingStatusSummary { get; set; } = null!;
    public DisputeSummaryDto DisputeSummary { get; set; } = null!;
    public List<MonthlyUserRegistrationDto> NewUsersByMonth { get; set; } = new();
    public List<TopRevenueTrainerDto> TopRevenueTrainers { get; set; } = new();
    public List<TopBookedPackageDto> TopBookedPackages { get; set; } = new();
}

public class DashboardKpiDto
{
    public int TotalUsers { get; set; }
    public int VerifiedPTs { get; set; }
    public int MonthlyBookings { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int PendingDisputes { get; set; }
    public int ActiveWorkspaces { get; set; }
    public int TotalPackages { get; set; }
    public int ActivePackages { get; set; }
}

public class BookingStatusSummaryDto
{
    public int PendingPayment { get; set; }
    public int PendingTrainerAcceptance { get; set; }
    public int Active { get; set; }
    public int CompletedPendingSettlement { get; set; }
    public int Settled { get; set; }
    public int Cancelled { get; set; }
    public int Refunded { get; set; }
    public int Disputed { get; set; }
}

public class DisputeSummaryDto
{
    public int Open { get; set; }
    public int UnderReview { get; set; }
    public int ResolvedRefundCustomer { get; set; }
    public int ResolvedReleaseToPT { get; set; }
    public int Withdrawn { get; set; }
}

public class MonthlyUserRegistrationDto
{
    public string Month { get; set; } = null!;
    public int Count { get; set; }
}

public class TopRevenueTrainerDto
{
    public Guid PtId { get; set; }
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}

public class TopBookedPackageDto
{
    public Guid PackageId { get; set; }
    public string PackageName { get; set; } = null!;
    public string TrainerName { get; set; } = null!;
    public int BookingCount { get; set; }
}
