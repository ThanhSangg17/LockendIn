using System;
using System.Collections.Generic;

namespace LockedIn.BusinessObject.DTOs.Workspaces;

public class WorkspaceProgressResponse
{
    public int TotalSessions { get; set; }
    public int CompletedSessionsCount { get; set; }
    public int RemainingSessions { get; set; }
    public List<WorkspaceSessionResponse> Sessions { get; set; } = new();
}
