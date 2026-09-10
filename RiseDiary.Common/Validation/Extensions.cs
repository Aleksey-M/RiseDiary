using FluentValidation.Results;

namespace RiseDiary.Common.Validation;

public static class Extensions
{
    public static string ErrorMessages(this ValidationResult? validationResult) =>
        string.Join(',', validationResult?.Errors.Select(x => $"{x.PropertyName} - {x.ErrorMessage};") ?? []);
}
