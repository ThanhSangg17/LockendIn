using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class AuditLog
{
    public Guid Id { get; set; }

    public Guid? ActorUserId { get; set; }

    public string Action { get; set; } = null!;

    public string? EntityName { get; set; }

    public Guid? EntityId { get; set; }

    public string? MetadataJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? ActorUser { get; set; }
}
