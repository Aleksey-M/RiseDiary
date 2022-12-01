using FluentValidation;
namespace RiseDiary.Shared.Images;

public sealed class UploadImageDto
{
    public string ImageName { get; set; } = "";

    public DateTime? Taken { get; set; }

    public string? CameraModel { get; set; }

    public Guid? TargetRecordId { get; set; }

    public int? NewBiggestDimension { get; set; }

    public string? ContentType { get; set; } = "";
}

public class UploadImageDtoValidator : AbstractValidator<UploadImageDto>
{
    public UploadImageDtoValidator()
    {
        RuleFor(x => x.ImageName)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrEmpty(x)).WithMessage("Название изображения обязательно")
            .MaximumLength(250).WithMessage("Название сферы интересов должно иметь не более {MaxLength} символов");

        RuleFor(x => x.NewBiggestDimension)
            .InclusiveBetween(50, 4000)
            .When(x => x.NewBiggestDimension.HasValue)
            .WithMessage("Допустимы значения от 50 до 4000");
    }
}