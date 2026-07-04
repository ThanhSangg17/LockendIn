using System.ComponentModel.DataAnnotations;

namespace LockedIn.BusinessObject.DTOs.Admin;

public class RejectProfileEditRequest
{
    [Required]
    public string Reason { get; set; } = null!;
}
