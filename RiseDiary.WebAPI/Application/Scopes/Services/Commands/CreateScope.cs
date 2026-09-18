namespace RiseDiary.WebAPI.Application.Scopes.Services.Commands;

public sealed class CreateScope : IScopeOperation
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
}
