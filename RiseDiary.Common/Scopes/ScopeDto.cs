namespace RiseDiary.Common.Scopes;

public sealed class ScopeDto
{
    public Guid? ScopeId { get; set; }
    public string? ScopeName { get; set; }
    public string? ScopeDescription { get; set; }
    public List<ThemeDto>? Themes { get; set; }

    public ScopeDto()
    {        
    }

    public ScopeDto(ScopeDto? source)
    {
        ScopeId = source?.ScopeId;
        ScopeDescription = source?.ScopeDescription;
        ScopeName = source?.ScopeName;
        Themes = source?.Themes?.Select(x => new ThemeDto(x)).ToList();
    }

    public void FillFrom(ScopeDto source)
    {
        ScopeId = source.ScopeId;
        ScopeName = source.ScopeName;
        ScopeDescription = source.ScopeDescription;
    }
}


public static class ScopeDtoExtensions
{
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