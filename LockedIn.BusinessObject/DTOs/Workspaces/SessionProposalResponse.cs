using System;
using System.Collections.Generic;

namespace LockedIn.BusinessObject.DTOs.Workspaces;

public class SessionProposalSlotResponse
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string SlotCode { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsSelected { get; set; }
    public bool IsAvailable { get; set; }
    public string? Reason { get; set; }
}

public class SessionProposalResponse
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public int SessionNumber { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SessionProposalSlotResponse> Slots { get; set; } = new();
}
