using System.ComponentModel.DataAnnotations;

namespace LockedIn.BusinessObject.DTOs.Reviews;

public class ReplyReviewRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Reply content is required.")]
    [StringLength(500, ErrorMessage = "Reply cannot exceed 500 characters.")]
    public string Reply { get; set; } = null!;
}
