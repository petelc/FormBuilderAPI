using DefaultValueAttribute = System.ComponentModel.DefaultValueAttribute;
using System.ComponentModel.DataAnnotations;
using FormBuilderAPI.Attributes;

namespace FormBuilderAPI.DTO;

public class RequestDTO<T> : IValidatableObject
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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validator = new SortColumnValidatorAttribute(typeof(T));
        var result = validator.GetValidationResult(SortColumn, validationContext);
        return (result != null) ? new[] { result } : new ValidationResult[0];
    }
}
