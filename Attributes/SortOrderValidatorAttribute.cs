using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace FormBuilderAPI.Attributes;

public class SortOrderValidatorAttribute : ValidationAttribute
{

    public string[] AllowedValues { get; set; } = new[] { "ASC", "DESC" };
    public SortOrderValidatorAttribute() : base("Value must be on of the following: {0}.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var strValue = value as string;
        if (!string.IsNullOrEmpty(strValue) && AllowedValues.Contains(strValue))
        {
            return ValidationResult.Success!;
        }

        return new ValidationResult(FormatErrorMessage(string.Join(", ", AllowedValues)));
    }
}
