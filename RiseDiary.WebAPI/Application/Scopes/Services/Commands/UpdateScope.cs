namespace RiseDiary.WebAPI.Application.Scopes.Services.Commands;

public sealed class UpdateScope : IScopeOperation
{
    public Guid ScopeId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
}
