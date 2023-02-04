using FluentValidation;

namespace RiseDiary.Shared.Records;

public sealed class UpdateRecordDto
{
    public Guid Id { get; set; }

    public DateOnly? Date { get; set; }

    public string? Name { get; set; }

    public string? Text { get; set; }
}


public sealed class UpdateRecordValidator : DtoValidator<UpdateRecordDto>
{
    public UpdateRecordValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(500).WithMessage("Длина названия записи не должна превышать 500 символов");

        RuleFor(x => x.Text)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(25000).WithMessage("Длина текста записи не должна превышать 25 000 символов");
    }
}


public static class UpdateRecordDtoExtensions
{
    public static void SetHostAndPortPlaceholder(this UpdateRecordDto dto, string baseUri)
    {
        if (dto.Name != null)
        {
            dto.Name = InternalLinksHelper.SetHostAndPortPlaceholder(dto.Name.Trim(), baseUri);
        }

        if (dto.Text != null)
        {
            dto.Text = InternalLinksHelper.SetHostAndPortPlaceholder(dto.Text.Trim(), baseUri);
        }
    }

    public static void SetBaseUri(this UpdateRecordDto dto, string baseUri)
    {
        if (dto.Name != null)
        {
            dto.Name = InternalLinksHelper.SetBaseUri(dto.Name.Trim(), baseUri);
        }

        if (dto.Text != null)
        {
            dto.Text = InternalLinksHelper.SetBaseUri(dto.Text.Trim(), baseUri);
        }
    }
}