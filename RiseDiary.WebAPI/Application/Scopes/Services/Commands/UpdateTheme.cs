namespace RiseDiary.WebAPI.Application.Scopes.Services.Commands;

public sealed class UpdateTheme : IScopeOperation
{
    public Guid ThemeId { get; init; }
    public string? Name { get; init; }
    public bool? Actual { get; init; }
    public string? Description { get; init; }
}
