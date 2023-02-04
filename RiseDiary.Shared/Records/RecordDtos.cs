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

    public List<ScopeDto> AllScopes { get; set; } = new();

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


public static class RecordViewExtensions
{
    public static void SetBaseUri(this RecordListItemDto dto, string baseUri)
    {
        dto.Name = InternalLinksHelper.SetBaseUri(dto.Name?.Trim() ?? string.Empty, baseUri);
    }

    public static void SetBaseUri(this RecordDto dto, string baseUri)
    {
        dto.Name = InternalLinksHelper.SetBaseUri(dto.Name?.Trim() ?? string.Empty, baseUri);
        dto.Text = InternalLinksHelper.SetBaseUri(dto.Text?.Trim() ?? string.Empty, baseUri);
        dto.Themes.ForEach(x => x.SetBaseUri(baseUri));
        dto.Cogitations.ForEach(x => x.SetBaseUri(baseUri));
    }

    public static void SetBaseUri(this RecordEditDto dto, string baseUri)
    {
        dto.Name = InternalLinksHelper.SetBaseUri(dto.Name?.Trim() ?? string.Empty, baseUri);
        dto.Text = InternalLinksHelper.SetBaseUri(dto.Text?.Trim() ?? string.Empty, baseUri);
        dto.Themes.ForEach(x => x.SetBaseUri(baseUri));
        dto.Cogitations.ForEach(x => x.SetBaseUri(baseUri));
        dto.AllScopes.ForEach(x => x.SetBaseUri(baseUri));
    }

    public static void SetBaseUri(this CogitationDto dto, string baseUri)
    {
        dto.Text = InternalLinksHelper.SetBaseUri(dto.Text?.Trim() ?? string.Empty, baseUri);
    }
}