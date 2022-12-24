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