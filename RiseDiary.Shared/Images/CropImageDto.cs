using FluentValidation;

namespace RiseDiary.Shared.Images;

public sealed class CropImageDto
{
    public Guid ImageId { get; set; }

    public int Left { get; set; }

    public int Top { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public int SourceImageWidth { get; set; }

    public int SourceImageHeight { get; set; }
}


public class CropImageDtoValidator : AbstractValidator<CropImageDto>
{
    public CropImageDtoValidator()
    {
        RuleFor(x => x.ImageId)
            .Cascade(CascadeMode.Stop)
            .NotEqual(Guid.Empty)
            .WithMessage("Не указан Id изображения");

        RuleFor(x => x.Left)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Top)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Width)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(50)
            .WithMessage("Ширина выделенной области не должна быть меньше 50 пикселей");

        RuleFor(x => x.Height)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(50)
            .WithMessage("Высота выделенной области не должна быть меньше 50 пикселей");

        RuleFor(x => x)
            .Must(x => x.Left + x.Width <= x.SourceImageWidth
                && x.Top + x.Height <= x.SourceImageHeight)
            .WithMessage("Некорректное выделение области");
    }
}
