using FluentValidation.Results;

namespace RiseDiary.Shared;

public interface IDtoValidator<T>
{
    ValidationResult ValidateForCreate(T dto, bool andThrow = false);

    ValidationResult ValidateForUpdate(T dto, bool andThrow = false);
}
