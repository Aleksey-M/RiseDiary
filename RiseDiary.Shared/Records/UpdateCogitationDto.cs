using FluentValidation;

namespace RiseDiary.Shared.Records;

public sealed class UpdateCogitationDto
{
    public Guid RecordId { get; set; }

    public Guid CogitationId { get; set; }

    public string Text { get; set; } = string.Empty;
}


public sealed class UpdateCogitationValidator : DtoValidator<UpdateCogitationDto>
{
    public UpdateCogitationValidator()
    {
        RuleFor(x => x.RecordId)
            .Cascade(CascadeMode.Stop)
            .NotEqual(default(Guid)).WithMessage("Не указан Id записи");

        RuleFor(x => x.CogitationId)
           .Cascade(CascadeMode.Stop)
           .NotEqual(default(Guid)).WithMessage("Не указан Id осмысления (комментария)");

        RuleFor(x => x.Text)
            .Cascade(CascadeMode.Stop)
            .NotNull().NotEmpty().WithMessage("Текст записи не может быть пустым")
            .MaximumLength(25000).WithMessage("Длина текста записи не должна превышать 25 000 символов");
    }
}


public static class UpdateCogitationDtoExtensions
{
    public static void SetHostAndPortPlaceholder(this UpdateCogitationDto dto, string baseUri)
    {
        dto.Text = InternalLinksHelper.SetHostAndPortPlaceholder(dto.Text?.Trim() ?? string.Empty, baseUri);
    }
}