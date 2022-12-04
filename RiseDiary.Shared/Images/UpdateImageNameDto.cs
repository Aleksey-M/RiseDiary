using FluentValidation;

namespace RiseDiary.Shared.Images;

public sealed class UpdateImageNameDto
{
    public Guid ImageId { get; set; }

    public string ImageName { get; set; } = string.Empty;
}

public class UpdateImageNameDtoValidator : AbstractValidator<UpdateImageNameDto>
{
    public UpdateImageNameDtoValidator()
    {
        RuleFor(x => x.ImageId)
            .Cascade(CascadeMode.Stop)
            .NotEqual(Guid.Empty)
            .WithMessage("Не указан Id изображения");

        RuleFor(x => x.ImageName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty().WithMessage("Новое название не введено!")
            .MaximumLength(250).WithMessage("Максимальная длина названия - 250 символов");
    }
}