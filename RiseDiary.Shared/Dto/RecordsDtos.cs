using System.ComponentModel.DataAnnotations;
using RiseDiary.Shared.Images;
using RiseDiary.Shared.Scopes;

namespace RiseDiary.Shared.Dto;

public sealed class UpdateRecordThemesDto
{
    [Required]
    public Guid RecordId { get; set; }

    [Required]
    public Guid[] ThemesIds { get; set; } = Array.Empty<Guid>();

}

public sealed class CreateRecordDto
{
    [Required]
    public DateOnly Date { get; set; }

    public string RecordName { get; set; } = "";

    [Required, MaxLength(25_000)]
    public string RecordText { get; set; } = "";

    public Guid[] ThemesIds { get; set; } = Array.Empty<Guid>();
}

public sealed class RecordDto
{
    public Guid Id { get; set; }

    public DateOnly Date { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public string? Name { get; set; }

    public string Text { get; set; } = "";

    public ThemeDto[] Themes { get; set; } = Array.Empty<ThemeDto>();

    public ImageListItemDto[] Images { get; set; } = Array.Empty<ImageListItemDto>();

    public CogitationDto[] Cogitations { get; set; } = Array.Empty<CogitationDto>();
}

public sealed class CogitationDto
{
    public Guid Id { get; set; }

    public DateTime CreateDate { get; set; }

    public string Text { get; set; } = "";
}

public sealed class UpdateRecordDto
{
    [Required]
    public Guid Id { get; set; }

    public DateOnly? NewDate { get; set; }

    public string? NewName { get; set; }

    public string? NewText { get; set; }
}

public sealed class CreateCogitationDto
{
    [Required]
    public Guid RecordId { get; set; }

    [Required, MaxLength(25_000)]
    public string Text { get; set; } = "";
}

public sealed class UpdateCogitationDto
{
    [Required]
    public Guid RecordId { get; set; }

    [Required]
    public Guid CogitationId { get; set; }

    [Required, MaxLength(25_000)]
    public string NewText { get; set; } = "";
}
