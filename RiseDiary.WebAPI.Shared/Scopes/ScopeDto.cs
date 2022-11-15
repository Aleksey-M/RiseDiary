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
}