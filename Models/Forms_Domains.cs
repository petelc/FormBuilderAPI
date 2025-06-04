using System;
using System.ComponentModel.DataAnnotations;

namespace FormBuilderAPI.Models;

public class Forms_Domains
{
    [Key]
    [Required]
    public int FormId { get; set; }

    [Key]
    [Required]
    public int DomainId { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Form? Form { get; set; } = null!;
    public Domain? Domain { get; set; } = null!;
}
