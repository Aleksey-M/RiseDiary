using FluentValidation;
using FluentValidation.Results;

namespace RiseDiary.Shared;

public abstract class DtoValidator<T> : AbstractValidator<T>, IDtoValidator<T>
{
    public virtual ValidationResult ValidateForCreate(T dto, bool andThrow = false)
    {
        if (andThrow)
        {
            this.ValidateAndThrow(dto);
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
        }

        return this.Validate(dto, options => options.IncludeRuleSets("identifier", "name"));
    }
}
