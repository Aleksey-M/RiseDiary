using System.ComponentModel.DataAnnotations;

namespace RiseDiary.Shared.Records;

public sealed class CreateCogitationDto
{
    [Required]
    public Guid RecordId { get; set; }

    [Required, MaxLength(25_000)]
    public string Text { get; set; } = "";
}
