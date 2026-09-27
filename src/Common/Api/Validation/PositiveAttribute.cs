using System.ComponentModel.DataAnnotations;

namespace Common.Api.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class PositiveAttribute : ValidationAttribute
{
    public PositiveAttribute() : base("{0} must be greater than zero")
    {
    }

    public override bool IsValid(object? value) => value switch
    {
        // Missing values are left to [Required], matching the built-in attributes.
        null => true,
        int i => i > 0,
        long l => l > 0,
        decimal d => d > 0,
        double d => d > 0,
        _ => false
    };
}
