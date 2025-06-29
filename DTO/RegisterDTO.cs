using System;
using System.ComponentModel.DataAnnotations;

namespace FormBuilderAPI.DTO;

public class RegisterDTO
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(12, ErrorMessage = "Password must be at least 12 characters long.")]
    public string Password { get; set; } = string.Empty;
}
