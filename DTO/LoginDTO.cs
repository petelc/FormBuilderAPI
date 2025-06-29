using System;
using System.ComponentModel.DataAnnotations;

namespace FormBuilderAPI.DTO;

public class LoginDTO
{
    [Required]
    [MaxLength(255)]
    public string? UserName { get; set; }
    [Required]
    [MinLength(12, ErrorMessage = "Password must be at least 12 characters long.")]
    public string? Password { get; set; }
}
