namespace RiseDiary.Shared.Scopes;

public sealed class ThemeDto
{
    public Guid? ThemeId { get; set; }

    public Guid? ScopeId { get; set; }

    public string? ThemeName { get; set; }

    public string? ThemeDescription { get; set; }

    public bool? Actual { get; set; }
}


public static class ThemeDtoExtensions
{
    public static ThemeDto Copy(this ThemeDto? source) => new()
    {
        ThemeId = source?.ThemeId,
        ScopeId = source?.ScopeId,
        ThemeDescription = source?.ThemeDescription,
        ThemeName = source?.ThemeName,
        Actual = source?.Actual
    };

    public static void FillFrom(this ThemeDto target, ThemeDto source)
    {
        target.ScopeId = source.ScopeId;
        target.ThemeId = source.ThemeId;
        target.ThemeDescription = source.ThemeDescription;
        target.ThemeName = source.ThemeName;
        target.Actual = source.Actual;
    }

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