using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormBuilderAPITests.TestData;

[Table("Domains")]
public class Domain
{
    [Key]
    [Required]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = null!;

    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    public ICollection<Forms_Domains>? Forms_Domains { get; set; }

}