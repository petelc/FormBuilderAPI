using CsvHelper.Configuration.Attributes;


namespace FormBuilderAPI.Models.CSV;

public class FrmRecord
{
    [Name("ID")]
    public int? ID { get; set; }

    [Name("Form Number")]
    public string? FormNumber { get; set; }

    [Name("Form Title")]
    public string? FormTitle { get; set; }

    [Name("Form Owner Division")]
    public string? FormOwnerDivision { get; set; }

    [Name("Form Owner")]
    public string? FormOwner { get; set; }

    [Name("Version")]
    public string? Version { get; set; }

    [Name("Created Date")]
    public DateTime? CreatedDate { get; set; }

    [Name("Revised Date")]
    public DateTime? RevisedDate { get; set; }

    [Name("Configuration Path")]
    public string? ConfigurationPath { get; set; }

    [Name("Domain")]
    public string? Domains { get; set; }


}
