using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared;
using RiseDiary.Shared.Scopes;

namespace RiseDiary.WebAPI.Controllers.ScopesArea;

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
    public async Task<ActionResult<Guid>> CreateScope([FromServices] IDtoValidator<ScopeDto> validator, ScopeDto dto)
    {
        validator.ValidateForCreate(dto, true);

        var id = await _scopeService.AddScope(
            newScopeName: dto.ScopeName!,
            newScopeDescription: dto.ScopeDescription ?? string.Empty);

        var hostAndPort = await _appSettingsService.GetHostAndPort();
        var newScopeUri = $@"{hostAndPort}/api/scopes/{id}";
        return Created(newScopeUri, id);
    }

    [HttpPost("{sid}/themes")]
    public async Task<ActionResult<Guid>> CreateTheme([FromServices] ThemeValidator validator, Guid sid, ThemeDto dto)
    {
        validator.ValidateForCreate(dto, true);

        if (sid != dto.ScopeId) return BadRequest(new { Message = "Not consistent request" });

        var newThemeId = await _scopeService.AddTheme(
            scopeId: sid,
            newThemeName: dto.ThemeName!,
            newThemeDescription: dto.ThemeDescription ?? string.Empty,
            actual: dto.Actual!.Value);

        var hostAndPort = await _appSettingsService.GetHostAndPort();
        var scopeUri = $@"{hostAndPort}/api/scopes/{sid}";
        return Created(scopeUri, newThemeId);
    }

    [HttpGet]
    public async Task<ActionResult<List<ScopeDto>>> GetScopes(bool? actual, CancellationToken cancellationToken)
    {
        var scopes = await _scopeService.GetScopes(actual, cancellationToken);
        return scopes.Select(s => s.ToDto()).ToList();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateScope([FromServices] IDtoValidator<ScopeDto> validator, Guid id, ScopeDto dto)
    {
        validator.ValidateForUpdate(dto, true);

        if (id != dto.ScopeId) return BadRequest(new { Message = "Not consistent request" });

        await _scopeService.UpdateScope(
            scopeId: dto.ScopeId.Value,
            scopeNewName: dto.ScopeName,
            scopeNewDescription: dto.ScopeDescription);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteScope(Guid id)
    {
        if (!await _scopeService.CanDeleteScope(id)) return BadRequest(new { Message = "Нельзя удалить сферу интересов с темами" });

        await _scopeService.DeleteScope(id);

        return NoContent();
    }

    [HttpPut("{scopeId}/themes/{themeId}")]
    public async Task<ActionResult> UpdateTheme([FromServices] ThemeValidator validator, Guid scopeId, Guid themeId, ThemeDto dto)
    {
        validator.ValidateForUpdate(dto, true);

        if (scopeId != dto.ScopeId || themeId != dto.ThemeId) return BadRequest(new { Message = "Not consistent request" });

        await _scopeService.UpdateTheme(
            themeId: dto.ThemeId.Value,
            themeNewName: dto.ThemeName,
            themeNewDescription: dto.ThemeDescription,
            themeActuality: dto.Actual);

        return NoContent();
    }

    [HttpPut("{scopeId}/themes/{themeId}/actuality")]
    public async Task<ActionResult> UpdateThemeActuality([FromServices] ThemeValidator validator, Guid scopeId, Guid themeId, ThemeDto dto)
    {
        validator.ValidateForActuality(dto, true);

        if (scopeId != dto.ScopeId || themeId != dto.ThemeId) return BadRequest(new { Message = "Not consistent request" });

        await _scopeService.UpdateTheme(themeId: dto.ThemeId.Value, themeActuality: dto.Actual);

        return NoContent();
    }

    [HttpDelete("{scopeId}/themes/{themeId}")]
    public async Task<IActionResult> DeleteTheme(Guid scopeId, Guid themeId)
    {
        await _scopeService.DeleteTheme(themeId);

        return NoContent();
    }
}
