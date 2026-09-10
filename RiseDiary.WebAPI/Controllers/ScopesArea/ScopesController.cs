using Microsoft.AspNetCore.Mvc;
using RiseDiary.Common.Core;
using RiseDiary.Common.Scopes;
using RiseDiary.Common.Validation;
using RiseDiary.Shared;
using RiseDiary.WebAPI.Controllers.ScopesArea;
using RiseDiary.WebAPI.Scopes.Services;

[ApiController]
[Route("api/scopes")]
public sealed class ScopesController : ControllerBase
{
    private readonly IScopesService _scopeService;

    public ScopesController(IScopesService scopesService)
    {
        _scopeService = scopesService;
    }

    [HttpPost]
    public async Task<ActionResult<Result<Guid>>> CreateScope(
        [FromServices] IDtoValidator<ScopeDto> validator,
        ScopeDto dto)
    {
        var validation = validator.ValidateForCreate(dto, true);
        if (!validation.IsValid)
        {
            return BadRequest(Result<Guid>.Failure(validation.ErrorMessages()));
        }

        var id = await _scopeService.AddScope(
            newScopeName: dto.ScopeName!,
            newScopeDescription: dto.ScopeDescription ?? string.Empty);

        return Ok(Result<Guid>.Success(id));
    }

    [HttpPost("{sid}/themes")]
    public async Task<ActionResult<Result<Guid>>> CreateTheme(
        [FromServices] ThemeDtoValidator validator,
        Guid sid,
        ThemeDto dto)
    {
        var validation = validator.ValidateForCreate(dto, true);
        if (!validation.IsValid)
        {
            return BadRequest(Result<Guid>.Failure(validation.ErrorMessages()));
        }

        if (sid != dto.ScopeId)
            return BadRequest(Result<Guid>.Failure("Not consistent request"));

        var newThemeId = await _scopeService.AddTheme(
            scopeId: sid,
            newThemeName: dto.ThemeName!,
            newThemeDescription: dto.ThemeDescription ?? string.Empty,
            actual: dto.Actual!.Value);

        return Ok(Result<Guid>.Success(newThemeId));
    }

    [HttpGet]
    public async Task<ActionResult<Result<ScopeDto[]>>> GetScopes(
        bool? actual,
        CancellationToken cancellationToken)
    {
        var scopes = await _scopeService.GetScopes(actual, cancellationToken);
        return Ok(Result<ScopeDto[]>.Success(scopes.Select(s => s.ToDto()).ToArray()));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Result>> UpdateScope(
        [FromServices] IDtoValidator<ScopeDto> validator,
        Guid id,
        ScopeDto dto)
    {
        var validation = validator.ValidateForUpdate(dto, true);
        if (!validation.IsValid)
        {
            return BadRequest(Result.Failure(validation.ErrorMessages()));
        }

        if (id != dto.ScopeId)
            return BadRequest(Result.Failure("Not consistent request"));

        await _scopeService.UpdateScope(
            scopeId: dto.ScopeId.Value,
            scopeNewName: dto.ScopeName,
            scopeNewDescription: dto.ScopeDescription);

        return Ok(Result.Success());
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result>> DeleteScope(Guid id)
    {
        if (!await _scopeService.CanDeleteScope(id))
            return BadRequest(Result.Failure("Нельзя удалить сферу интересов с темами"));

        await _scopeService.DeleteScope(id);
        return Ok(Result.Success());
    }

    [HttpPut("{scopeId}/themes/{themeId}")]
    public async Task<ActionResult<Result>> UpdateTheme(
        [FromServices] ThemeDtoValidator validator,
        Guid scopeId,
        Guid themeId,
        ThemeDto dto)
    {
        var validation = validator.ValidateForUpdate(dto, true);
        if (!validation.IsValid)
        {
            return BadRequest(Result.Failure(validation.ErrorMessages()));
        }

        if (scopeId != dto.ScopeId || themeId != dto.ThemeId)
            return BadRequest(Result.Failure("Not consistent request"));

        await _scopeService.UpdateTheme(
            themeId: dto.ThemeId.Value,
            themeNewName: dto.ThemeName,
            themeNewDescription: dto.ThemeDescription,
            themeActuality: dto.Actual);

        return Ok(Result.Success());
    }

    [HttpPut("{scopeId}/themes/{themeId}/actuality")]
    public async Task<ActionResult<Result>> UpdateThemeActuality(
        [FromServices] ThemeDtoValidator validator,
        Guid scopeId,
        Guid themeId,
        ThemeDto dto)
    {
        var validation = validator.ValidateForActuality(dto, true);
        if (!validation.IsValid)
        {
            return BadRequest(Result.Failure(validation.ErrorMessages()));
        }

        if (scopeId != dto.ScopeId || themeId != dto.ThemeId)
            return BadRequest(Result.Failure("Not consistent request"));

        await _scopeService.UpdateTheme(themeId: dto.ThemeId.Value, themeActuality: dto.Actual);

        return Ok(Result.Success());
    }

    [HttpDelete("{scopeId}/themes/{themeId}")]
    public async Task<ActionResult<Result>> DeleteTheme(Guid scopeId, Guid themeId)
    {
        await _scopeService.DeleteTheme(themeId);
        return Ok(Result.Success());
    }
}