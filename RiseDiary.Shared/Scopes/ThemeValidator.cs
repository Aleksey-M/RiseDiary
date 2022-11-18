using FluentValidation;
using FluentValidation.Results;

namespace RiseDiary.Shared.Scopes;

public sealed class ThemeValidator : DtoValidator<ThemeDto>
{
    public ThemeValidator()
    {
        RuleSet("name", () =>
        {
            RuleFor(x => x.ThemeName)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Название темы обязательно")
                .NotEmpty().WithMessage("Название темы обязательно")
                .MaximumLength(100).WithMessage("Название темы должно иметь не более {MaxLength} символов");

            RuleFor(x => x.ThemeDescription)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(2000).WithMessage("Описание должно иметь не более {MaxLength} символов");
        });        

        RuleSet("identifier", () =>
        {
            RuleFor(x => x.ThemeId)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Не указан Id записи")
                .NotEqual(Guid.Empty).WithMessage("Не указан Id записи");
        });

        RuleSet("parentIdentifier", () =>
        {
            RuleFor(x => x.ScopeId)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Не указан Id родительской записи")
                .NotEqual(Guid.Empty).WithMessage("Не указан Id родительской записи");
        });

        RuleSet("actuality", () =>
        {
            RuleFor(x => x.Actual)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Значение актуальности отсутствует");
        });
    }

    public override ValidationResult ValidateForCreate(ThemeDto dto, bool andThrow = false) =>
        this.Validate(dto, options =>
        {
            options.IncludeRuleSets("parentIdentifier", "name", "actuality");
            options.IncludeRuleSets();

            if (andThrow)
            {
                options.ThrowOnFailures();
            }
        });

    public override ValidationResult ValidateForUpdate(ThemeDto dto, bool andThrow = false) =>
        this.Validate(dto, options =>
        {
            options.IncludeRuleSets("*");

            if (andThrow)
            {
                options.ThrowOnFailures();
            }
        });


    public ValidationResult ValidateForActuality(ThemeDto dto, bool andThrow = false) =>
        this.Validate(dto, options =>
        {
            options.IncludeRuleSets("identifier", "parentIdentifier", "actuality");

            if (andThrow)
            {
                options.ThrowOnFailures();
            }
        });
}
