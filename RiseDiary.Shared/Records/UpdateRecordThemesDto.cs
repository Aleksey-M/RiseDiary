using System.ComponentModel.DataAnnotations;

namespace RiseDiary.Shared.Records;

public sealed class UpdateRecordThemesDto
{
    [Required]
    public Guid RecordId { get; set; }

    [Required]
    public Guid[] ThemesIds { get; set; } = Array.Empty<Guid>();
}
