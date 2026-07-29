using System;

namespace LockedIn.DataAccess.Models;

public partial class SessionProposalSlot
{
    public Guid Id { get; set; }

    public Guid ProposalId { get; set; }

    public DateTime Date { get; set; }

    public string SlotCode { get; set; } = null!;

    public bool IsSelected { get; set; }

    public virtual SessionProposal Proposal { get; set; } = null!;
}
