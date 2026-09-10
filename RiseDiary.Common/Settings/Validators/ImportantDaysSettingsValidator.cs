using FluentValidation;

namespace RiseDiary.Common.Settings;

public sealed class ImportantDaysSettingsValidator : AbstractValidator<ImportantDaysSettingsDto>
{
    public ImportantDaysSettingsValidator()
    {
        RuleFor(x => x.ImportantDaysDisplayRange)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(1, 35).WithMessage("Значение диапазона отображаемых дат должно быть от 1 до 35");
    }
}