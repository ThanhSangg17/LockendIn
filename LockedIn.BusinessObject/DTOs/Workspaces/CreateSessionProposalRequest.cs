using System;
using System.Collections.Generic;

namespace LockedIn.BusinessObject.DTOs.Workspaces;

public class CreateProposalSlotDto
{
    public DateTime Date { get; set; }
    public string SlotCode { get; set; } = null!;
}

public class CreateSessionProposalRequest
{
    public int SessionNumber { get; set; }
    public List<CreateProposalSlotDto> Slots { get; set; } = new();
}
