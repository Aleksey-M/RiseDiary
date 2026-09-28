using Microsoft.AspNetCore.Components;
using RiseDiary.Common.Abstractions.Clients;
using RiseDiary.Common.Core;
using RiseDiary.Common.Scopes;
using RiseDiary.Common.Validation;
using RiseDiary.Shared;
using System.Net.Http.Json;
using System.Text.Json;

namespace RiseDiary.WebUI.Clients;

public sealed class ScopesClient : ClientBase, IScopesClient
{
    private readonly HttpClient _http;
    private readonly NavigationManager _navManager;
    private readonly IDtoValidator<ScopeDto> _scopeValidator;
    private readonly IDtoValidator<ThemeDto> _themeValidator;

    public ScopesClient(
        HttpClient http,
        JsonSerializerOptions jsonOptions,
        NavigationManager navManager,
        IDtoValidator<ScopeDto> scopeValidator,
        IDtoValidator<ThemeDto> themeValidator) : base(jsonOptions)
    {
        _http = http;
        _navManager = navManager;
        _scopeValidator = scopeValidator;
        _themeValidator = themeValidator;
    }

    public async Task<Result<Guid>> CreateScope(ScopeDto dto)
    {
        var validation = _scopeValidator.ValidateForCreate(dto);
        if (!validation.IsValid)
        {
            return Result<Guid>.Failure(validation.ErrorMessages());
        }

        var response = await _http.PostAsJsonAsync("api/scopes", dto);
        return await ReadResult<Guid>(response);
    }

    public async Task<Result<Guid>> CreateTheme(ThemeDto dto)
    {
        var validation = _themeValidator.ValidateForCreate(dto);
        if (!validation.IsValid)
        {
            return Result<Guid>.Failure(validation.ErrorMessages());
        }

        var response = await _http.PostAsJsonAsync($"api/scopes/{dto.ScopeId}/themes", dto);
        return await ReadResult<Guid>(response);
    }

    public async Task<Result<List<ScopeDto>>> GetScopes(CancellationToken ct = default)
    {
        var response = await _http.GetAsync("api/scopes", ct);
        var result = await ReadResult<List<ScopeDto>>(response);
        if (result.Succeeded && result.Data != null)
        {
            result.Data.ForEach(x => x.SetBaseUri(_navManager.BaseUri));
        }

        return result;
    }

    public async Task<Result> UpdateScope(ScopeDto dto)
    {
        var validation = _scopeValidator.ValidateForUpdate(dto);
        if (!validation.IsValid)
        {
            return Result<Guid>.Failure(validation.ErrorMessages());
        }

        var response = await _http.PutAsJsonAsync($"api/scopes/{dto.ScopeId}", dto);
        return await ReadResult(response);
    }

    public async Task<Result> DeleteScope(Guid id)
    {
        var response = await _http.DeleteAsync($"api/scopes/{id}");
        return await ReadResult(response);
    }

    public async Task<Result> UpdateTheme(ThemeDto dto)
    {
        var validation = _themeValidator.ValidateForUpdate(dto);
        if (!validation.IsValid)
        {
            return Result<Guid>.Failure(validation.ErrorMessages());
        }

        var response = await _http.PutAsJsonAsync($"api/scopes/{dto.ScopeId}/themes/{dto.ThemeId}", dto);
        return await ReadResult(response);
    }

    public async Task<Result> DeleteTheme(Guid scopeId, Guid themeId)
    {
        var response = await _http.DeleteAsync($"api/scopes/{scopeId}/themes/{themeId}");
        return await ReadResult(response);
    }
}

