using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class SessionProposal
{
    public Guid Id { get; set; }

    public Guid WorkspaceId { get; set; }

    public int SessionNumber { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Workspace Workspace { get; set; } = null!;

    public virtual ICollection<SessionProposalSlot> SessionProposalSlots { get; set; } = new List<SessionProposalSlot>();
}
