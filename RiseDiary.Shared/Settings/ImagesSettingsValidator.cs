using FluentValidation;

namespace RiseDiary.Shared.Settings;

public sealed class ImagesSettingsValidator : AbstractValidator<ImagesSettings>
{
    public ImagesSettingsValidator()
    {
        RuleFor(x => x.ImageQuality)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(1, 100).WithMessage("Значение сжатия Jpeg должно быть в пределах от 1 до 100");

        RuleFor(x => x.ThumbnailSize)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(50, 500).WithMessage("Размер превью-изображений должен быть в предлах от 50 до 500");

        RuleFor(x => x.CropImageMaxScaledHeight)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(200, 2000).WithMessage("Высота миниатюры на странице обрезки изображения должна быть от 200 до 2000");

        RuleFor(x => x.CropImageMaxScaledWidth)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(200, 2000).WithMessage("Ширина миниатюры на странице обрезки изображения должна быть от 200 до 2000");
    }
}