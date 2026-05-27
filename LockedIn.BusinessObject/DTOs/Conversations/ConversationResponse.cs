using System;

namespace LockedIn.BusinessObject.DTOs.Conversations;

public class ConversationResponse
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid PtProfileId { get; set; }
    public string FirebaseConversationId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
