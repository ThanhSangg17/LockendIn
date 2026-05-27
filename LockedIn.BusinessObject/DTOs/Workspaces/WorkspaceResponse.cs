using System;

namespace LockedIn.BusinessObject.DTOs.Workspaces;

public class WorkspaceResponse
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid PtProfileId { get; set; }
    public int Status { get; set; }
    public string? CourseNote { get; set; }
    public DateTime CreatedAt { get; set; }
}
