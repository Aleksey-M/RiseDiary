using System.ComponentModel.DataAnnotations;

namespace RiseDiary.Shared.Records;

public sealed class UpdateRecordDto
{
    [Required]
    public Guid Id { get; set; }

    public DateOnly? NewDate { get; set; }

    public string? NewName { get; set; }

    public string? NewText { get; set; }
}
