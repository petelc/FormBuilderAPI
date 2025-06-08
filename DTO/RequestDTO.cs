using FormBuilderAPI.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FormBuilderAPI.DTO;

public class RequestDTO
{
    [DefaultValue(0)]
    public int PageIndex { get; set; } = 0;
    [DefaultValue(10)]
    public int PageSize { get; set; } = 10;

    [DefaultValue("FormNumber")]
    [SortColumnValidator(typeof(FormDTO))]
    public string? SortColumn { get; set; } = "FormNumber";

    [DefaultValue("ASC")]
    [SortOrderValidator]
    public string? SortOrder { get; set; } = "ASC";

    [DefaultValue(null)]
    public string? FilterQuery { get; set; } = null;
}
