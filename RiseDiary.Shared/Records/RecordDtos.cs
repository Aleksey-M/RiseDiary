using RiseDiary.Shared.Images;
using RiseDiary.Shared.Scopes;

namespace RiseDiary.Shared.Records;

public class RecordListItemDto
{
    public Guid RecordId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateOnly Date { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }
}


public sealed class RecordDto : RecordListItemDto
{
    public string Text { get; set; } = string.Empty;

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


public sealed class RecordsPageDto<T>
{
    public PagesInfo PagesInfo { get; set; } = null!;

    public List<T> Records { get; set; } = null!;
}