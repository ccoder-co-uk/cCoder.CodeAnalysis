// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Validations;

namespace cCoder.CodeAnalysis.Sample.Services.Processings.Validations;

internal static class ValidationRulesEngine
{
    internal static void Validate(IEnumerable<object?> inputs)
    {
        ValidationRule[] rules = inputs
            .Select(
                selector: (object? input) =>
                    new ValidationRule
                    {
                        IsInvalid = () => input == null,
                        CreateException = () => new ArgumentNullException(nameof(input)),
                    }
            )
            .ToArray();

        using IEnumerator<ValidationRule> enumerator = rules
            .Where(predicate: (ValidationRule validationRule) => validationRule.IsInvalid())
            .GetEnumerator();

        if (enumerator.MoveNext())
        {
            ValidationRule rule = enumerator.Current;
            throw rule.CreateException();
        }
    }
}