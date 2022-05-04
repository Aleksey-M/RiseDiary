using Microsoft.EntityFrameworkCore;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.Model.Services
{
    internal sealed class ScopesService : IScopesService
    {
        private readonly DiaryDbContext _context;

        public ScopesService(DiaryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Guid> AddScope(string newScopeName)
        {
            if (string.IsNullOrWhiteSpace(newScopeName)) throw new ArgumentException($"Parameter {nameof(newScopeName)} should not be null or empty");
            newScopeName = newScopeName.Trim();

            if (await _context.Scopes.AnyAsync(s => s.ScopeName == newScopeName)) throw new ArgumentException($"Scope with name {newScopeName} already exists");

            var scope = new DiaryScope
            {
                Id = Guid.NewGuid(),
                ScopeName = newScopeName.Trim()
            };
            await _context.Scopes.AddAsync(scope).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return scope.Id;
        }

        public async Task<Guid> AddTheme(Guid scopeId, string newThemeName, bool actual)
        {
            if (string.IsNullOrWhiteSpace(newThemeName)) throw new ArgumentException($"Parameter {nameof(newThemeName)} should not be null or empty");
            newThemeName = newThemeName.Trim();

            var scope = await _context.Scopes.AsNoTracking().Include(s => s.Themes).SingleOrDefaultAsync(s => s.Id == scopeId).ConfigureAwait(false);
            if (scope == null) throw new ArgumentException($"Scope with id={scopeId} is not exists");

            if (scope.Themes.Any(t => t.ThemeName == newThemeName)) throw new ArgumentException($"Theme with name '{newThemeName}' already exists in '{scope.ScopeName}' scope");

            var theme = new DiaryTheme { Id = Guid.NewGuid(), ScopeId = scopeId, ThemeName = newThemeName.Trim(), Actual = actual };
            await _context.Themes.AddAsync(theme).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return theme.Id;
        }

        public async Task<bool> CanDeleteScope(Guid scopeId)
        {
            bool themesExist = await _context.Themes.AnyAsync(th => th.ScopeId == scopeId).ConfigureAwait(false);
            return !themesExist;
        }

        public async Task DeleteScope(Guid scopeId)
        {
            bool canDelete = await CanDeleteScope(scopeId).ConfigureAwait(false);
            if (!canDelete) return;

            var scope = await _context.Scopes
                .Include(s => s.Themes)
                .ThenInclude(t => t.RecordsRefs)
                .SingleOrDefaultAsync(s => s.Id == scopeId)
                .ConfigureAwait(false);

            if (scope != null && !scope.Themes.Any(t => !t.Deleted))
            {
                _context.Scopes.Remove(scope);
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task DeleteTheme(Guid themeId)
        {
            var theme = await _context.Themes
                .Include(t => t.RecordsRefs)
                .SingleOrDefaultAsync(t => t.Id == themeId)
                .ConfigureAwait(false);

            if (theme != null)
            {
                _context.Themes.Remove(theme);
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<DiaryScope?> FetchScopeById(Guid scopeId)
        {
            return await _context.Scopes
                .AsNoTracking()
                .Include(s => s.Themes)
                .ThenInclude(t => t.RecordsRefs)
                .SingleOrDefaultAsync(s => s.Id == scopeId)
                .ConfigureAwait(false);
        }

        public async Task<List<DiaryScope>> GetScopes(bool? themesActuality = null)
        {
            var scopesList = await _context.Scopes
                    .Include(s => s.Themes)
                    .ThenInclude(t => t.RecordsRefs)
                    .AsNoTracking()
                    .OrderBy(s => s.ScopeName)
                    .ToListAsync()
                    .ConfigureAwait(false);

            if (themesActuality == null) return scopesList;

            foreach (var scope in scopesList.ToList())
            {
                scope.Themes = scope.Themes.Where(t => t.Actual == themesActuality.Value).ToList();
                if (scope.Themes.Count == 0) scopesList.Remove(scope);
            }

            return scopesList;
        }

        public async Task UpdateScopeName(Guid scopeId, string scopeNewName)
        {
            if (string.IsNullOrWhiteSpace(scopeNewName)) throw new ArgumentException("Scope name should not be empty", nameof(scopeNewName));
            scopeNewName = scopeNewName.Trim();

            if (await _context.Scopes.AnyAsync(s => s.ScopeName == scopeNewName && s.Id != scopeId)) throw new ArgumentException($"Scope with name {scopeNewName} already exists");

            var targetScope = await _context.Scopes.FindAsync(scopeId).ConfigureAwait(false);
            if (targetScope == null) throw new ArgumentException($"Scope with id = {scopeId} is not exists");
            if (targetScope.Deleted) throw new ArgumentException($"Scope with id = {scopeId} is deleted");

            targetScope.ScopeName = scopeNewName.Trim();
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateTheme(Guid themeId, string? themeNewName = null, bool? themeActuality = null)
        {
            if (string.IsNullOrWhiteSpace(themeNewName) && themeActuality == null) return;

            var targetTheme = await _context.Themes.FindAsync(themeId);
            if (targetTheme == null) throw new ArgumentException($"Theme with id = {themeId} is not exists");
            if (targetTheme.Deleted) throw new ArgumentException($"Theme with id = {themeId} is deleted");

            if (!string.IsNullOrWhiteSpace(themeNewName))
            {
                if (await _context.Themes.AnyAsync(t => t.ScopeId == targetTheme.ScopeId && t.Id != targetTheme.Id && t.ThemeName == themeNewName)) throw new ArgumentException($"Theme with name {themeNewName} already exists");
                targetTheme.ThemeName = themeNewName;
            }

            if (themeActuality != null && targetTheme.Actual != themeActuality)
            {
                targetTheme.Actual = themeActuality.Value;
            }

            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
