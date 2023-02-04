using FluentValidation;

namespace RiseDiary.Shared.Records;

public sealed class CreateCogitationDto
{
    public Guid RecordId { get; set; }

    public string Text { get; set; } = string.Empty;
}


public sealed class CreateCogitationValidator : DtoValidator<CreateCogitationDto>
{
    public CreateCogitationValidator()
    {
        RuleFor(x => x.RecordId)
            .Cascade(CascadeMode.Stop)
            .NotEqual(default(Guid)).WithMessage("Не указан Id записи");

        RuleFor(x => x.Text)
            .Cascade(CascadeMode.Stop)
            .NotNull().NotEmpty().WithMessage("Текст записи не может быть пустым")
            .MaximumLength(25000).WithMessage("Длина текста записи не должна превышать 25 000 символов");
    }
}

public static class CreateCogitationDtoExtensions
{
    public static void SetHostAndPortPlaceholder(this CreateCogitationDto dto, string baseUri)
    {
        dto.Text = InternalLinksHelper.SetHostAndPortPlaceholder(dto.Text?.Trim() ?? string.Empty, baseUri);
    }
}