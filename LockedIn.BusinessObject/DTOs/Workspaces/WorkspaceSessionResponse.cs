using System;

namespace LockedIn.BusinessObject.DTOs.Workspaces;

public class WorkspaceSessionResponse
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public int SessionNumber { get; set; }
    public string? Description { get; set; }
    public DateTime CompletedAt { get; set; }
}
