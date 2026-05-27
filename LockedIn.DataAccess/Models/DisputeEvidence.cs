using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class DisputeEvidence
{
    public Guid Id { get; set; }

    public Guid DisputeId { get; set; }

    public string FileUrl { get; set; } = null!;

    public string? FileType { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual Dispute Dispute { get; set; } = null!;
}
