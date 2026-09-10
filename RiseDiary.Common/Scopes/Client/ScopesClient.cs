using RiseDiary.Common.Core;
using RiseDiary.Common.Validation;
using System.Net.Http.Json;
using System.Text.Json;

namespace RiseDiary.Common.Scopes.Client;

public sealed class ScopesClient : ScopesClientBase, IScopesClient
{
    private readonly HttpClient _http;
    private readonly ScopeDtoValidator _scopeValidator;
    private readonly ThemeDtoValidator _themeValidator;

    public ScopesClient(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        ScopeDtoValidator scopeValidator,
        ThemeDtoValidator themeValidator) : base(jsonOptions)
    {
        _http = http;
        _scopeValidator = scopeValidator;
        _themeValidator = themeValidator;
    }

    public async Task<Result<Guid>> CreateScope(ScopeDto dto, CancellationToken ct = default)
    {
        var validation = _scopeValidator.ValidateForCreate(dto, true);
        if (!validation.IsValid)
        {
            return Result<Guid>.Failure(validation.ErrorMessages());
        }

        var response = await _http.PostAsJsonAsync("api/scopes", dto, ct);
        return await ReadResult<Guid>(response);
    }

    public async Task<Result<Guid>> CreateTheme(Guid scopeId, ThemeDto dto, CancellationToken ct = default)
    {
        var validation = _themeValidator.ValidateForCreate(dto, true);
        if (!validation.IsValid)
        {
            return Result<Guid>.Failure(validation.ErrorMessages());
        }

        var response = await _http.PostAsJsonAsync($"api/scopes/{scopeId}/themes", dto, ct);
        return await ReadResult<Guid>(response);
    }

    public async Task<Result<List<ScopeDto>>> GetScopes(bool? actual = null, CancellationToken ct = default)
    {
        var url = actual.HasValue ? $"api/scopes?actual={actual}" : "api/scopes";
        var response = await _http.GetAsync(url, ct);
        return await ReadResult<List<ScopeDto>>(response);
    }

    public async Task<Result> UpdateScope(Guid id, ScopeDto dto, CancellationToken ct = default)
    {
        var validation = _scopeValidator.ValidateForUpdate(dto, true);
        if (!validation.IsValid)
        {
            return Result<Guid>.Failure(validation.ErrorMessages());
        }

        var response = await _http.PutAsJsonAsync($"api/scopes/{id}", dto, ct);
        return await ReadResult(response);
    }

    public async Task<Result> DeleteScope(Guid id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/scopes/{id}", ct);
        return await ReadResult(response);
    }

    public async Task<Result> UpdateTheme(Guid scopeId, Guid themeId, ThemeDto dto, CancellationToken ct = default)
    {
        var validation = _themeValidator.ValidateForUpdate(dto, true);
        if (!validation.IsValid)
        {
            return Result<Guid>.Failure(validation.ErrorMessages());
        }

        var response = await _http.PutAsJsonAsync($"api/scopes/{scopeId}/themes/{themeId}", dto, ct);
        return await ReadResult(response);
    }

    public async Task<Result> UpdateThemeActuality(Guid scopeId, Guid themeId, ThemeDto dto, CancellationToken ct = default)
    {
        var validation = _themeValidator.ValidateForActuality(dto, true);
        if (!validation.IsValid)
        {
            return Result<Guid>.Failure(validation.ErrorMessages());
        }

        var response = await _http.PutAsJsonAsync(
            $"api/scopes/{scopeId}/themes/{themeId}/actuality",
            dto,
            ct);

        return await ReadResult(response);
    }

    public async Task<Result> DeleteTheme(Guid scopeId, Guid themeId, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/scopes/{scopeId}/themes/{themeId}", ct);
        return await ReadResult(response);
    }
}

