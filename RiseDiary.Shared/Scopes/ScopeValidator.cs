using FluentValidation;

namespace RiseDiary.Shared.Scopes;

public sealed class ScopeValidator : DtoValidator<ScopeDto>
{
    public ScopeValidator()
    {
        RuleSet("name", () =>
        {
            RuleFor(x => x.ScopeName)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Название сферы интересов обязательно")
                .NotEmpty().WithMessage("Название сферы интересов обязательно")
                .MaximumLength(100).WithMessage("Название сферы интересов должно иметь не более {MaxLength} символов");

            RuleFor(x => x.ScopeDescription)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(2000).WithMessage("Описание должно иметь не более {MaxLength} символов");
        });        

        RuleSet("identifier", () =>
        {
            RuleFor(x => x.ScopeId)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Не указан Id записи")
                .NotEqual(Guid.Empty).WithMessage("Не указан Id записи");
        });
    }
}
