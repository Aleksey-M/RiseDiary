using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Api
{
    [ApiController]
    public class ScopesController : ControllerBase
    {
        private readonly DiaryDbContext _context;
        public ScopesController(DiaryDbContext context)
        {
            _context = context;
        }

        [HttpGet, Route("api/v1.0/scopes")]
        [ProducesResponseType(typeof(IEnumerable<ScopeItemDto>), StatusCodes.Status200OK)]
        public async IAsyncEnumerable<ScopeItemDto> GetScopes()
        {
            var scopes = await _context.GetAllScopes();
            foreach (var s in scopes)
            {
                yield return new ScopeItemDto { ScopeId = s.Id, ScopeName = s.ScopeName };
            }
        }

        [HttpGet, Route("api/v1.0/scopes/count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<int> GetScopesCount()
        {
            return await _context.GetScopesCount();
        }

        [HttpGet, Route("api/v1.0/scopes/{id}")]
        [ProducesResponseType(typeof(ScopeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ScopeDto>> GetScope(Guid id)
        {
            var scope = await _context.FetchScopeById(id);
            if (scope == null)
            {
                return NotFound();
            }

            return new ScopeDto
            {
                ScopeId = scope.Id,
                ScopeName = scope.ScopeName,
                Themes = scope.Themes.Select(t => new ThemeDto { ThemeId = t.Id, ThemeName = t.ThemeName, Actual = t.Actual })
            };
        }

        [HttpPost, Route("api/v1.0/scopes")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ScopeDto>> CreateScope(NewScopeDto newScopeDto)
        {
            bool alreadyExists = await _context.Scopes.AnyAsync(s => s.ScopeName == newScopeDto.NewScopeName.Trim());
            if (alreadyExists)
            {
                return BadRequest($"Scope with name {newScopeDto.NewScopeName} already exist");
            }

            var id = await _context.AddScope(newScopeDto.NewScopeName);

            var scope = new ScopeDto
            {
                ScopeId = id,
                ScopeName = newScopeDto.NewScopeName,
                Themes = Enumerable.Empty<ThemeDto>()
            };

            return CreatedAtAction(nameof(GetScope), new { id }, scope);
        }

        [HttpPut, Route("api/v1.0/scopes/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateScope(Guid id, UpdateScopeDto dto)
        {
            if (id != dto.ScopeId)
            {
                return BadRequest("Incorrect Id");
            }

            var scope = await _context.FetchScopeById(id);
            if (scope == null)
            {
                return NotFound();
            }

            scope.ScopeName = dto.NewScopeName;
            await _context.UpdateScope(scope);

            return NoContent();
        }

        [HttpDelete, Route("api/v1.0/scopes/{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ScopeDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ScopeDto>> DeleteScope(Guid id)
        {
            var scope = await _context.FetchScopeById(id);
            if (scope == null)
            {
                return NotFound();
            }

            if (!(await _context.CanDeleteScope(id)))
            {
                return BadRequest("Scope can not be deleted. Associated themes exists");
            }

            var dto = new ScopeDto
            {
                ScopeId = scope.Id,
                ScopeName = scope.ScopeName,
                Themes = Enumerable.Empty<ThemeDto>()
            };

            await _context.DeleteScope(id);

            return dto;
        }

        [HttpGet, Route("api/v1.0/scopes/{scopeId}/themes/count")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetThemesCount(Guid scopeId)
        {
            var scope = await _context.FetchScopeById(scopeId);
            if (scope == null)
            {
                return NotFound();
            }

            return scope.Themes.Count;
        }

        [HttpGet, Route("api/v1.0/scopes/{scopeId}/theme")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ThemeDto>>> GetThemes(Guid scopeId)
        {
            var scope = await _context.FetchScopeById(scopeId);
            if (scope == null)
            {
                return NotFound();
            }

            return scope.Themes.Select(t => new ThemeDto { ThemeId = t.Id, ThemeName = t.ThemeName, Actual = t.Actual }).ToList();
        }

        [HttpGet, Route("api/v1.0/scopes/{scopeId}/theme/{themeId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ThemeDto>> GetTheme(Guid scopeId, Guid themeId)
        {
            var scope = await _context.FetchScopeById(scopeId);
            if (scope == null)
            {
                return NotFound();
            }

            var theme = await _context.FetchThemeById(themeId);
            if (theme == null)
            {
                return NotFound();
            }

            if (theme.ScopeId != scopeId)
            {
                return BadRequest("Not consistent request");
            }

            return new ThemeDto { ThemeId = theme.Id, ThemeName = theme.ThemeName, Actual = theme.Actual };
        }

        [HttpPost, Route("api/v1.0/scopes/{scopeId}/theme")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ThemeDto>> CreateTheme(Guid scopeId, NewThemeDto dto)
        {
            if (scopeId != dto.ScopeId)
            {
                return BadRequest("Not consistent request");
            }

            var scope = await _context.FetchScopeById(scopeId);
            if (scope == null)
            {
                return NotFound();
            }

            bool alreadyExists = await _context.Themes.AnyAsync(t => t.ScopeId == scopeId && t.ThemeName == dto.NewThemeName.Trim());
            if (alreadyExists)
            {
                return BadRequest($"Theme with name '{dto.NewThemeName}' already exist in scope '{scope.ScopeName}'");
            }

            var newThemeId = await _context.AddTheme(scopeId, dto.NewThemeName);
            var themeDto = new ThemeDto { ThemeId = newThemeId, ThemeName = dto.NewThemeName, Actual = false };

            return CreatedAtAction(nameof(GetTheme), new { scopeId, themeId = newThemeId }, themeDto);
        }

        [HttpPut, Route("api/v1.0/scopes/{scopeId}/theme/{themeId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateTheme(Guid scopeId, Guid themeId, UpdateThemeDto dto)
        {
            var scope = await _context.FetchScopeById(scopeId);
            var theme = await _context.FetchThemeById(dto.ThemeId);
            if (scope == null || theme == null)
            {
                return NotFound();
            }

            if (scopeId != dto.ScopeId || themeId != dto.ThemeId || dto.ScopeId != scope.Id)
            {
                return BadRequest("Not consistent request");
            }

            bool alreadyExists = await _context.Themes.AnyAsync(t => t.ScopeId == scopeId && t.ThemeName == dto.ThemeNewName.Trim());
            if (alreadyExists)
            {
                return BadRequest($"Theme with name '{dto.ThemeNewName}' already exist in scope '{scope.ScopeName}'");
            }

            theme.ThemeName = dto.ThemeNewName;
            theme.Actual = dto.NewActual;
            await _context.UpdateTheme(theme);

            return NoContent();
        }

        [HttpDelete, Route("api/v1.0/scopes/{scopeId}/theme/{themeId}")]
        public async Task<ActionResult<ThemeDto>> DeleteTheme(Guid scopeId, Guid themeId)
        {
            var scope = await _context.FetchScopeById(scopeId);
            var theme = await _context.FetchThemeById(themeId);
            if (scope == null || theme == null)
            {
                return NotFound();
            }

            if (theme.ScopeId != scope.Id)
            {
                return BadRequest("Not consistent request");
            }

            await _context.DeleteTheme(themeId);

            return new ThemeDto { ThemeId = theme.Id, ThemeName = theme.ThemeName, Actual = theme.Actual };
        }
    }

    public class ScopeItemDto
    {
        public Guid ScopeId { get; set; }
        public string ScopeName { get; set; } = null!;
    }

    public class ThemeDto
    {
        public Guid ThemeId { get; set; }
        public string ThemeName { get; set; } = null!;
        public bool Actual { get; set; }
    }

    public class ScopeDto
    {
        [Required]
        public Guid ScopeId { get; set; }
        [Required, StringLength(50)]
        public string ScopeName { get; set; } = null!;
        public IEnumerable<ThemeDto> Themes { get; set; } = null!;
    }

    public class UpdateScopeDto
    {
        [Required]
        public Guid ScopeId { get; set; }
        [Required, StringLength(50)]
        public string NewScopeName { get; set; } = null!;
    }

    public class NewScopeDto
    {
        [Required, StringLength(50)]
        public string NewScopeName { get; set; } = null!;
    }

    public class NewThemeDto
    {
        [Required]
        public Guid ScopeId { get; set; }
        [Required, StringLength(50)]
        public string NewThemeName { get; set; } = null!;
    }

    public class UpdateThemeDto
    {
        [Required]
        public Guid ScopeId { get; set; }
        [Required]
        public Guid ThemeId { get; set; }
        [Required, StringLength(50)]
        public string ThemeNewName { get; set; } = null!;
        [Required]
        public bool NewActual { get; set; }
    }
}
