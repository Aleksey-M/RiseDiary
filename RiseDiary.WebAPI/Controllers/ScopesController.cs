﻿using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.WebAPI.Shared.Dto;

namespace RiseDiary.Api;

[ApiController]
[Route("api/scopes")]
public sealed class ScopesController : ControllerBase
{
    private readonly IScopesService _scopeService;

    private readonly IAppSettingsService _appSettingsService;

    public ScopesController(IScopesService scopesService, IAppSettingsService appSettingsService)
    {
        _scopeService = scopesService;
        _appSettingsService = appSettingsService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateScope(NewScopeDto newScopeDto)
    {
        var newScopeName = newScopeDto.NewScopeName.Trim();
        var id = await _scopeService.AddScope(newScopeName);
        var newScopeUri = $@"{await _appSettingsService.GetHostAndPort()}/api/scopes/{id}";
        return Created(newScopeUri, id);
    }

    [HttpGet, Route("{id}")]
    [ProducesResponseType(typeof(ScopeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScopeDto>> GetScope(Guid id, CancellationToken token)
    {
        var scope = await _scopeService.FetchScopeById(id, token);
        if (scope == null) return NotFound();

        return new ScopeDto
        {
            ScopeId = scope.Id,
            ScopeName = scope.ScopeName,
            Themes = scope.Themes.Select(t => new ThemeDto
            {
                ThemeId = t.Id,
                ThemeName = t.ThemeName,
                Actual = t.Actual
            })
        };
    }

    [HttpPost, Route("{sid}/themes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateTheme(Guid sid, NewThemeDto dto)
    {
        if (sid != dto.ScopeId) return BadRequest("Not consistent request");

        var newThemeId = await _scopeService.AddTheme(sid, dto.NewThemeName.Trim(), dto.Actual);
        var scopeUri = $@"{await _appSettingsService.GetHostAndPort()}/api/scopes/{sid}";
        return Created(scopeUri, newThemeId);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ScopeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ScopeDto>>> GetScopes(bool? actual, CancellationToken cancellationToken)
    {
        var scopes = await _scopeService.GetScopes(actual, cancellationToken);

        return scopes.Select(s => new ScopeDto
        {
            ScopeId = s.Id,
            ScopeName = s.ScopeName,
            Themes = s.Themes.Select(t => new ThemeDto
            {
                ThemeId = t.Id,
                Actual = t.Actual,
                ThemeName = t.ThemeName
            })
        })
        .ToList();
    }

    [HttpPut, Route("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateScope(Guid id, UpdateScopeDto dto)
    {
        if (id != dto.ScopeId) return BadRequest("Not consistent request");

        await _scopeService.UpdateScope(dto.ScopeId, dto.NewScopeName);
        return NoContent();
    }

    [HttpDelete, Route("{id}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteScope(Guid id)
    {
        if (!await _scopeService.CanDeleteScope(id)) return BadRequest("Scope can not be deleted. Associated themes exists");

        var scope = await _scopeService.FetchScopeById(id);
        if (scope != null) await _scopeService.DeleteScope(id);
        return NoContent();
    }

    [HttpPut, Route("{scopeId}/themes/{themeId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateTheme(Guid scopeId, Guid themeId, UpdateThemeDto dto)
    {
        if (scopeId != dto.ScopeId || themeId != dto.ThemeId) return BadRequest("Not consistent request");

        await _scopeService.UpdateTheme(dto.ThemeId, dto.ThemeNewName, dto.NewActual);
        return NoContent();
    }

    [HttpDelete, Route("{scopeId}/themes/{themeId}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteTheme(Guid scopeId, Guid themeId)
    {
        var scope = (await _scopeService.GetScopes()).SingleOrDefault(s => s.Id == scopeId);

        if (scope != null) await _scopeService.DeleteTheme(themeId);
        return NoContent();
    }
}
