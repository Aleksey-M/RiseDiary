namespace RiseDiary.Shared.Scopes;

public sealed class ScopeDto
{
    public Guid? ScopeId { get; set; }

    public string? ScopeName { get; set; }

    public string? ScopeDescription { get; set; }

    public List<ThemeDto>? Themes { get; set; }
}


public static class ScopeDtoExtensions
{
    public static ScopeDto Copy(this ScopeDto? source) => new()
    {
        ScopeId = source?.ScopeId,
        ScopeDescription = source?.ScopeDescription,
        ScopeName = source?.ScopeName,
        Themes = source?.Themes
    };

    public static void FillFrom(this ScopeDto target, ScopeDto source)
    {
        target.ScopeId = source.ScopeId;
        target.ScopeName = source.ScopeName;
        target.ScopeDescription = source.ScopeDescription;
    }

    public static void SetHostAndPortPlaceholder(this ScopeDto dto, string baseUri)
    {
        dto.ScopeDescription = InternalLinksHelper.SetHostAndPortPlaceholder(
            dto.ScopeDescription?.Trim() ?? string.Empty,
            baseUri);
    }

    public static void SetBaseUri(this ScopeDto dto, string baseUri)
    {
        dto.ScopeDescription = InternalLinksHelper.SetBaseUri(
            dto.ScopeDescription?.Trim() ?? string.Empty,
            baseUri);
    }

    public static void SetBaseUri(this List<ScopeDto> dtoList, string baseUri)
    {
        foreach (var scope in dtoList)
        {
            scope.SetBaseUri(baseUri);
            scope.Themes?.ForEach(x => x.SetBaseUri(baseUri));
        }
    }
}