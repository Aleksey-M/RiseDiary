using Microsoft.AspNetCore.Mvc;
using RiseDiary.Common.Core;
using RiseDiary.Common.Scopes;
using RiseDiary.Common.Validation;
using RiseDiary.Shared;
using RiseDiary.WebAPI.Application.Scopes.Abstraction;
using RiseDiary.WebAPI.Application.Scopes.Services.Commands;
using RiseDiary.WebAPI.Controllers.ScopesArea;

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

        var command = new CreateScope
        {
            Name = dto.ScopeName ?? string.Empty,
            Description = dto.ScopeDescription,
        };

        var result = await _scopeService.Apply(command);
        return Ok(Result<Guid>.FormResult(result, d => d.NewEntityId ?? default));
    }

    [HttpPost("{sid}/themes")]
    public async Task<ActionResult<Result<Guid>>> CreateTheme(
        [FromServices] ThemeDtoValidator validator,
        Guid sid,
        ThemeDto dto)
    {
        dto.ScopeId = sid;
        var validation = validator.ValidateForCreate(dto, true);
        if (!validation.IsValid)
        {
            return BadRequest(Result<Guid>.Failure(validation.ErrorMessages()));
        }

        var command = new CreateTheme
        {
            ScopeId = dto.ScopeId.Value,
            Name = dto.ThemeName ?? string.Empty,
            Description = dto.ThemeDescription,
            Actual = dto.Actual ?? false
        };

        var result = await _scopeService.Apply(command);
        return Ok(Result<Guid>.FormResult(result, d => d.NewEntityId ?? default));
    }

    [HttpGet]
    public async Task<ActionResult<Result<ScopeDto[]>>> GetScopes(CancellationToken cancellationToken)
    {
        var result = await _scopeService.GetScopes(cancellationToken);

        return Ok(Result<ScopeDto[]>.FormResult(
            result,
            data => result.Data!.Select(s => s.ToDto()).ToArray()));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Result>> UpdateScope(
        [FromServices] IDtoValidator<ScopeDto> validator,
        Guid id,
        ScopeDto dto)
    {
        dto.ScopeId = id;

        var validation = validator.ValidateForUpdate(dto, true);
        if (!validation.IsValid)
        {
            return BadRequest(Result.Failure(validation.ErrorMessages()));
        }

        var command = new UpdateScope
        {
            ScopeId = dto.ScopeId.Value,
            Description = dto.ScopeDescription,
            Name = dto.ScopeName
        };

        return Ok(await _scopeService.Apply(command));
    }

    [HttpPut("{scopeId}/themes/{themeId}")]
    public async Task<ActionResult<Result>> UpdateTheme(
        [FromServices] ThemeDtoValidator validator,
        Guid scopeId,
        Guid themeId,
        ThemeDto dto)
    {
        dto.ScopeId = scopeId;
        dto.ThemeId = themeId;

        var validation = validator.ValidateForUpdate(dto, true);
        if (!validation.IsValid)
        {
            return BadRequest(Result.Failure(validation.ErrorMessages()));
        }

        var command = new UpdateTheme
        {
            ThemeId = dto.ThemeId.Value,
            Name = dto.ThemeName,
            Description = dto.ThemeDescription,
            Actual = dto.Actual ?? false
        };

        return Ok(await _scopeService.Apply(command));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result>> DeleteScope(Guid id)
    {
        return Ok(await _scopeService.DeleteScope(id));
    }

    [HttpDelete("{scopeId}/themes/{themeId}")]
    public async Task<ActionResult<Result>> DeleteTheme(Guid scopeId, Guid themeId)
    {
        return Ok(await _scopeService.DeleteTheme(themeId));
    }
}