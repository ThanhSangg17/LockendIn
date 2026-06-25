using System.ComponentModel.DataAnnotations;

namespace LockedIn.BusinessObject.DTOs.Auth;

public class GoogleLoginRequest
{
    [Required(ErrorMessage = "Google ID token is required.")]
    public string IdToken { get; set; } = null!;

    public int? Role { get; set; }
}
