using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class PtDocument
{
    public Guid Id { get; set; }

    public Guid PtProfileId { get; set; }

    public int DocumentType { get; set; }

    public string FileUrl { get; set; } = null!;

    public int Status { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual PtProfile PtProfile { get; set; } = null!;
}
