using System;

namespace LockedIn.BusinessObject.DTOs.Admin;

public class AuditLogResponse
{
    public Guid Id { get; set; }
    public Guid? ActorUserId { get; set; }
    public string Action { get; set; } = null!;
    public string? EntityName { get; set; }
    public Guid? EntityId { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; }
}
