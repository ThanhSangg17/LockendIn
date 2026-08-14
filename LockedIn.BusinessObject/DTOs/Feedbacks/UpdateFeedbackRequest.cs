using System.ComponentModel.DataAnnotations;

namespace LockedIn.BusinessObject.DTOs.Feedbacks;

public class UpdateFeedbackRequest
{
    [Required(ErrorMessage = "Nội dung góp ý không được để trống.")]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "Nội dung góp ý phải từ 5 đến 2000 ký tự.")]
    public string Content { get; set; } = null!;
}
