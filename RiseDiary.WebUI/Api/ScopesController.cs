using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Api
{
    [ApiController]
    [Authorize]
    public sealed class ScopesController : ControllerBase
    {
        private readonly IScopesService _scopeService;

        private readonly IAppSettingsService _appSettingsService;

        public ScopesController(IScopesService scopesService, IAppSettingsService appSettingsService)
        {
            _scopeService = scopesService;
            _appSettingsService = appSettingsService;
        }

        [HttpPost, Route("api/v1.0/scopes")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> CreateScope(NewScopeDto newScopeDto)
        {
            try
            {
                var newScopeName = newScopeDto.NewScopeName.Trim();
                var id = await _scopeService.AddScope(newScopeName);
                var newScopeUri = $@"{await _appSettingsService.GetHostAndPort()}/api/v1.0/scopes/{id}";
                return Created(newScopeUri, id);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpGet, Route("api/v1.0/scopes/{id}")]
        [ProducesResponseType(typeof(ScopeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ScopeDto>> GetScope(Guid id)
        {
            var scope = await _scopeService.FetchScopeById(id);
            if (scope == null) return NotFound();

            return new ScopeDto
            {
                ScopeId = scope.Id,
                ScopeName = scope.ScopeName,
                Themes = scope.Themes.Select(t => new ThemeDto { ThemeId = t.Id, ThemeName = t.ThemeName, Actual = t.Actual })
            };
        }

        [HttpPost, Route("api/v1.0/scopes/{sid}/themes")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> CreateTheme(Guid sid, NewThemeDto dto)
        {
            if (sid != dto.ScopeId) return BadRequest("Not consistent request");

            try
            {
                var newThemeId = await _scopeService.AddTheme(sid, dto.NewThemeName.Trim(), dto.Actual);
                var scopeUri = $@"{await _appSettingsService.GetHostAndPort()}/api/v1.0/scopes/{sid}";
                return Created(scopeUri, newThemeId);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpGet, Route("api/v1.0/scopes")]
        [ProducesResponseType(typeof(IEnumerable<ScopeDto>), StatusCodes.Status200OK)]
        public async Task<List<ScopeDto>> GetScopes(bool? actual)
        {
            var scopes = await _scopeService.GetScopes(actual);

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
            }).ToList();
        }

        [HttpPut, Route("api/v1.0/scopes/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateScope(Guid id, UpdateScopeDto dto)
        {
            if (id != dto.ScopeId) return BadRequest("Not consistent request");

            try
            {
                await _scopeService.UpdateScopeName(dto.ScopeId, dto.NewScopeName);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }

            return NoContent();
        }

        [HttpDelete, Route("api/v1.0/scopes/{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteScope(Guid id)
        {
            if (!await _scopeService.CanDeleteScope(id)) return BadRequest("Scope can not be deleted. Associated themes exists");

            try
            {
                var scope = await _scopeService.FetchScopeById(id);
                if (scope != null) await _scopeService.DeleteScope(id);
                return NoContent();
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpPut, Route("api/v1.0/scopes/{scopeId}/themes/{themeId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateTheme(Guid scopeId, Guid themeId, UpdateThemeDto dto)
        {
            if (scopeId != dto.ScopeId || themeId != dto.ThemeId) return BadRequest("Not consistent request");

            try
            {
                await _scopeService.UpdateTheme(dto.ThemeId, dto.ThemeNewName, dto.NewActual);
                return NoContent();
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpDelete, Route("api/v1.0/scopes/{scopeId}/themes/{themeId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteTheme(Guid scopeId, Guid themeId)
        {
            var scope = (await _scopeService.GetScopes(null)).SingleOrDefault(s => s.Id == scopeId);

            try
            {
                if (scope != null) await _scopeService.DeleteTheme(themeId);
                return NoContent();
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }
    }
}
