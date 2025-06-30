using FluentValidation;

namespace RiseDiary.Shared.Settings;

public sealed class PagesSizesSettings
{
    public int ImagesPageSize { get; set; }

    public int RecordsPageSize { get; set; }

    public int AvailableImagesPageSize { get; set; }
}

public sealed class PagesSizesSettingsValidator : AbstractValidator<PagesSizesSettings>
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