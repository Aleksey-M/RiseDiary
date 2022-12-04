using FluentValidation;

namespace RiseDiary.Shared.Images;

public sealed class ScaleDownImageDto
{
    public Guid ImageId { get; set; }

    public int DimensionMaxLimit { get; set; }

    public int ImageMaxDimension { get; set; }
}


public class ScaleDownImageDtoValidator : AbstractValidator<ScaleDownImageDto>
{
    public ScaleDownImageDtoValidator()
    {
        RuleFor(x => x.ImageId)
            .Cascade(CascadeMode.Stop)
            .NotEqual(Guid.Empty)
            .WithMessage("Не указан Id изображения");

        RuleFor(x => x.ImageMaxDimension)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0)
            .WithMessage("Большая сторона изображения не указана");

        RuleFor(x => x.DimensionMaxLimit)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(10, 5500)
            .WithMessage("Допустимый диапазон значения - от 10 до 5500 пикселей");

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(x => x.ImageMaxDimension >= x.DimensionMaxLimit)
            .WithMessage("Указанный максимальный предел больше текущего размера изображения");
    }
}