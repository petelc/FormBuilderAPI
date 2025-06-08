using System;
using System.ComponentModel.DataAnnotations;

namespace FormBuilderAPI.Attributes;

public class SortColumnValidatorAttribute : ValidationAttribute
{
    public Type EntityType { get; set; }

    public SortColumnValidatorAttribute(Type entityType) : base("Value must be a valid column name.")
    {
        EntityType = entityType;
    }
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (EntityType != null)
        {
            var strValue = value as string;
            if (!string.IsNullOrEmpty(strValue) && EntityType.GetProperties().Any(p => p.Name == strValue))
            {
                return ValidationResult.Success!;
            }
        }
        return new ValidationResult(FormatErrorMessage(ErrorMessage!));
    }
}
