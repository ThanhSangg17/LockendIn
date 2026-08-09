using System.ComponentModel.DataAnnotations;

namespace LockedIn.BusinessObject.DTOs.PtProfiles;

public class UpdatePtQrCodeRequest
{
    [Required(ErrorMessage = "QR code URL is required.")]
    [Url(ErrorMessage = "Invalid URL format for QR code.")]
    public string QrCodeUrl { get; set; } = null!;
}
