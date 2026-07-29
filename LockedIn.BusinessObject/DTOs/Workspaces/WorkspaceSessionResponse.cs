using System;

namespace LockedIn.BusinessObject.DTOs.Workspaces;

public class WorkspaceSessionResponse
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public int SessionNumber { get; set; }
    public int Status { get; set; }
    public DateTime? ScheduledStart { get; set; }
    public DateTime? ScheduledEnd { get; set; }
    public DateTime? PtCheckedInAt { get; set; }
    public DateTime? CustomerCheckedInAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public string? Description { get; set; }
    public DateTime? CompletedAt { get; set; }
}
