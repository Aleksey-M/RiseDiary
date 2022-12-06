using System.ComponentModel.DataAnnotations;

namespace RiseDiary.Shared.Records;

public sealed class UpdateCogitationDto
{
    [Required]
    public Guid RecordId { get; set; }

    [Required]
    public Guid CogitationId { get; set; }

    [Required, MaxLength(25_000)]
    public string NewText { get; set; } = "";
}
