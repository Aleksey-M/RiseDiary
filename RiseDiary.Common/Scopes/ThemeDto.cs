namespace RiseDiary.Common.Scopes;

public sealed class ThemeDto
{
    public Guid? ThemeId { get; set; }
    public Guid? ScopeId { get; set; }
    public string? ThemeName { get; set; }
    public string? ThemeDescription { get; set; }
    public bool? Actual { get; set; }

    public ThemeDto()
    {        
    }

    public ThemeDto(ThemeDto? source)
    {
        ThemeId = source?.ThemeId;
        ScopeId = source?.ScopeId;
        ThemeDescription = source?.ThemeDescription;
        ThemeName = source?.ThemeName;
        Actual = source?.Actual;
    }

    public void FillFrom(ThemeDto source)
    {
        ScopeId = source.ScopeId;
        ThemeId = source.ThemeId;
        ThemeDescription = source.ThemeDescription;
        ThemeName = source.ThemeName;
        Actual = source.Actual;
    }
}


public static class ThemeDtoExtensions
{

    public static void SetHostAndPortPlaceholder(this ThemeDto dto, string baseUri)
    {
        dto.ThemeDescription = InternalLinksHelper.SetHostAndPortPlaceholder(
            dto.ThemeDescription?.Trim() ?? string.Empty,
            baseUri);
    }

    public static void SetBaseUri(this ThemeDto dto, string baseUri)
    {
        dto.ThemeDescription = InternalLinksHelper.SetBaseUri(
            dto.ThemeDescription?.Trim() ?? string.Empty,
            baseUri);
    }
}