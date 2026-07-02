using System;

namespace LockedIn.DataAccess.Models;

public partial class WorkspaceSession
{
    public Guid Id { get; set; }

    public Guid WorkspaceId { get; set; }

    public int SessionNumber { get; set; }

    public string? Description { get; set; }

    public DateTime CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Workspace Workspace { get; set; } = null!;
}
