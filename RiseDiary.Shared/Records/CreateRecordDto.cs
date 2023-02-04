using FluentValidation;

namespace RiseDiary.Shared.Records;

public sealed class CreateRecordDto
{
    public DateOnly Date { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;
}


public sealed class CreateRecordValidator : DtoValidator<CreateRecordDto>
{
    public CreateRecordValidator()
    {
        RuleFor(x => x.Date)
            .Cascade(CascadeMode.Stop)
            .NotEqual(default(DateOnly)).WithMessage("Не указана дата записи");

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(500).WithMessage("Длина названия записи не должна превышать 500 символов");

        RuleFor(x => x.Text)
            .Cascade(CascadeMode.Stop)
            .NotNull().NotEmpty().WithMessage("Текст записи не может быть пустым")
            .MaximumLength(25000).WithMessage("Длина текста записи не должна превышать 25 000 символов");
    }
}

public static class CreateRecordDateExtensions
{
    public static void SetHostAndPortPlaceholder(this CreateRecordDto dto, string baseUri)
    {
        dto.Name = InternalLinksHelper.SetHostAndPortPlaceholder(dto.Name?.Trim() ?? string.Empty, baseUri);
        dto.Text = InternalLinksHelper.SetHostAndPortPlaceholder(dto.Text?.Trim() ?? string.Empty, baseUri);
    }
}