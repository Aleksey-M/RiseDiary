using FluentValidation;

namespace RiseDiary.Common.Settings;

public sealed class ImagesSettingsValidator : AbstractValidator<ImagesSettingsDto>
{
    public ImagesSettingsValidator()
    {
        RuleFor(x => x.ImageQuality)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(1, 100).WithMessage("Значение сжатия Jpeg должно быть в пределах от 1 до 100");

        RuleFor(x => x.ThumbnailSize)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(50, 500).WithMessage("Размер превью-изображений должен быть в предлах от 50 до 500");
    }
}
