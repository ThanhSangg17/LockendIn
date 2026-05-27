using System;

namespace LockedIn.BusinessObject.DTOs.Admin;

public class DashboardResponse
{
    public int TotalUsers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalPts { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OpenDisputes { get; set; }
}
