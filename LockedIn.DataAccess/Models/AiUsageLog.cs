using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class AiUsageLog
{
    public Guid Id { get; set; }

    public Guid PtProfileId { get; set; }

    public string Feature { get; set; } = null!;

    public int TokenUsed { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PtProfile PtProfile { get; set; } = null!;
}
