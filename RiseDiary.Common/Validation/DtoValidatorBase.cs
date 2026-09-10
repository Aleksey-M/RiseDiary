using FluentValidation;
using FluentValidation.Results;

namespace RiseDiary.Shared;

public abstract class DtoValidatorBase<T> : AbstractValidator<T>, IDtoValidator<T>
{
    public virtual ValidationResult ValidateForCreate(T dto, bool andThrow = false)
    {
        if (andThrow)
        {
            this.ValidateAndThrow(dto);
            return new ValidationResult();
        }

        return Validate(dto);
    }

    public virtual ValidationResult ValidateForUpdate(T dto, bool andThrow = false)
    {
        if (andThrow)
        {
            this.Validate(dto, options =>
            {
                options.IncludeRuleSets("identifier", "name");
                options.ThrowOnFailures();
            });

            return new ValidationResult();
        }

        return this.Validate(dto, options => options.IncludeRuleSets("identifier", "name"));
    }
}
