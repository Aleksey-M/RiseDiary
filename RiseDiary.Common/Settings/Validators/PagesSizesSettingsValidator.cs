using FluentValidation;

namespace RiseDiary.Common.Settings;

public sealed class PagesSizesSettingsValidator : AbstractValidator<PagesSizesSettingsDto>
{
    public PagesSizesSettingsValidator()
    {
        RuleFor(x => x.ImagesPageSize)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(1, 200).WithMessage("Допустимы значения от 1 до 200");

        RuleFor(x => x.RecordsPageSize)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(1, 200).WithMessage("Допустимы значения от 1 до 200");

        RuleFor(x => x.AvailableImagesPageSize)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(1, 200).WithMessage("Допустимы значения от 1 до 200");
    }
}