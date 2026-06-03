using System;
using System.ComponentModel.DataAnnotations;

namespace LockedIn.BusinessObject.DTOs.Disputes;

public class CreateDisputeRequest
{
    public Guid BookingId { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Reason is required.")]
    [StringLength(255, ErrorMessage = "Reason cannot exceed 255 characters.")]
    public string Reason { get; set; } = null!;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Description is required.")]
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string Description { get; set; } = null!;
}
