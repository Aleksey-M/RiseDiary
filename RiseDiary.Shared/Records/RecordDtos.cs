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


public class RecordDto : RecordListItemDto
{
    public string Text { get; set; } = string.Empty;

    public List<ThemeDto> Themes { get; set; } = new();

    public List<ImageListItemDto> Images { get; set; } = new();

    public List<CogitationDto> Cogitations { get; set; } = new();
}


public sealed class RecordEditDto : RecordDto
{
    public Guid? StartPageRecordId { get; set; }

    public ScopeDto[] AllScopes { get; set; } = Array.Empty<ScopeDto>();

    public int AddImagesPageSize { get; set; }
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