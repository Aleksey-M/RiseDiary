namespace RiseDiary.WebAPI.Application.Scopes.Services.Commands;

public sealed class CreateTheme : IScopeOperation
{
    public Guid ScopeId { get; init; }
    public string Name { get; init; } = null!;
    public bool Actual { get; init; }
    public string? Description { get; init; }
}
