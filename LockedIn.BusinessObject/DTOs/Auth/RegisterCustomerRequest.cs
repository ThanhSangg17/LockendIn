using System;
using System.ComponentModel.DataAnnotations;

namespace LockedIn.BusinessObject.DTOs.Auth;

public class RegisterCustomerRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters and contain at least one letter and one number.")]
    [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d).+$", ErrorMessage = "Password must be at least 8 characters and contain at least one letter and one number.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 150 characters.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Phone number must be exactly 10 digits and start with 0.")]
    public string Phone { get; set; } = null!;
}
