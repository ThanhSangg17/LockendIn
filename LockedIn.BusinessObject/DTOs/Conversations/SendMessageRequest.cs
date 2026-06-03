using System;
using System.ComponentModel.DataAnnotations;

namespace LockedIn.BusinessObject.DTOs.Conversations;

public class SendMessageRequest
{
    public Guid ConversationId { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Message content is required.")]
    [StringLength(1000, ErrorMessage = "Message content cannot exceed 1000 characters.")]
    public string Content { get; set; } = null!;

    public string MessageType { get; set; } = null!;
}
