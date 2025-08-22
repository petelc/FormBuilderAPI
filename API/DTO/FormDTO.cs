using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FormBuilderAPI.DTO;

public class FormDTO
{
    [Required]
    public int Id { get; set; }
    [Required]
    public string FormNumber { get; set; } = string.Empty;

    [Required]
    public string FormTitle { get; set; } = string.Empty;

    [Required]
    public string? FormOwnerDivision { get; set; } = string.Empty;

    [Required]
    public string? FormOwner { get; set; } = string.Empty;

    [Required]
    public string? Version { get; set; } = string.Empty;

    public DateTime? CreatedDate { get; set; }

    public DateTime? RevisedDate { get; set; }

    public string? ConfigurationPath { get; set; } = string.Empty;
}
