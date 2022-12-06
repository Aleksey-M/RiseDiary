using System.ComponentModel.DataAnnotations;

namespace RiseDiary.Shared.Records;

public sealed class CreateRecordDto
{
    [Required]
    public DateOnly Date { get; set; }

    public string RecordName { get; set; } = "";

    [Required, MaxLength(25_000)]
    public string RecordText { get; set; } = "";

    public Guid[] ThemesIds { get; set; } = Array.Empty<Guid>();
}
