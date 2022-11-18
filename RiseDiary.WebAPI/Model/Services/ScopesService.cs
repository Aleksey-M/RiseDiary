using Microsoft.EntityFrameworkCore;
using RiseDiary.Data;

namespace RiseDiary.Model.Services;

internal sealed class ScopesService : IScopesService
{
    private readonly DiaryDbContext _context;

    private readonly IAppSettingsService _appSettingsService;

    public ScopesService(DiaryDbContext context, IAppSettingsService appSettingsService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
    }

    public async Task<Guid> AddScope(string newScopeName, string newScopeDescription)
    {
        if (string.IsNullOrWhiteSpace(newScopeName))
            throw new ArgumentException($"Parameter {nameof(newScopeName)} should not be null or empty");

        newScopeName = newScopeName.Trim();
        newScopeDescription = newScopeDescription?.Trim() ?? "";

        if (await _context.Scopes.AnyAsync(s => s.ScopeName == newScopeName))
            throw new ArgumentException($"Scope with name {newScopeName} already exists");

        if (newScopeDescription != "")
        {
            var placeholder = _appSettingsService.GetHostAndPortPlaceholder();
            var currentHostAndPort = await _appSettingsService.GetHostAndPort();

            newScopeDescription =
                newScopeDescription.Replace(currentHostAndPort, placeholder, StringComparison.OrdinalIgnoreCase);
        }

        var scope = new DiaryScope
        {
            Id = Guid.NewGuid(),
            ScopeName = newScopeName,
            Description = newScopeDescription
        };

        await _context.Scopes.AddAsync(scope).ConfigureAwait(false);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        return scope.Id;
    }

    public async Task<Guid> AddTheme(Guid scopeId, string newThemeName, bool actual, string newThemeDescription)
    {
        if (string.IsNullOrWhiteSpace(newThemeName))
            throw new ArgumentException($"Parameter {nameof(newThemeName)} should not be null or empty");

        newThemeName = newThemeName.Trim();
        newThemeDescription = newThemeDescription?.Trim() ?? "";

        var scope = await _context.Scopes
            .AsNoTracking()
            .Include(s => s.Themes)
            .SingleOrDefaultAsync(s => s.Id == scopeId)
            .ConfigureAwait(false);

        if (scope == null) throw new ArgumentException($"Scope with id={scopeId} is not exists");

        if (scope.Themes.Any(t => t.ThemeName == newThemeName))
            throw new ArgumentException($"Theme with name '{newThemeName}' already exists in '{scope.ScopeName}' scope");

        if (newThemeDescription != "")
        {
            var placeholder = _appSettingsService.GetHostAndPortPlaceholder();
            var currentHostAndPort = await _appSettingsService.GetHostAndPort();

            newThemeDescription =
                newThemeDescription.Replace(currentHostAndPort, placeholder, StringComparison.OrdinalIgnoreCase);
        }

        var theme = new DiaryTheme
        {
            Id = Guid.NewGuid(),
            ScopeId = scopeId,
            ThemeName = newThemeName.Trim(),
            Actual = actual,
            Description = newThemeDescription
        };

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

    public async Task<DiaryScope?> FetchScopeById(Guid scopeId, CancellationToken cancellationToken)
    {
        return await _context.Scopes
            .AsNoTracking()
            .Include(s => s.Themes)
            .ThenInclude(t => t.RecordsRefs)
            .SingleOrDefaultAsync(s => s.Id == scopeId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<DiaryScope>> GetScopes(bool? themesActuality, CancellationToken cancellationToken)
    {
        var scopesList = await _context.Scopes
            .Include(s => s.Themes)
            .ThenInclude(t => t.RecordsRefs)
            .AsNoTracking()
            .OrderBy(s => s.ScopeName)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (themesActuality != null)
        {
            foreach (var scope in scopesList.ToList())
            {
                cancellationToken.ThrowIfCancellationRequested();

                scope.Themes = scope.Themes.Where(t => t.Actual == themesActuality.Value).ToList();
                if (scope.Themes.Count == 0) scopesList.Remove(scope);
            }
        }

        var placeholder = _appSettingsService.GetHostAndPortPlaceholder();
        var currentHostAndPort = await _appSettingsService.GetHostAndPort();

        foreach (var scope in scopesList)
        {
            cancellationToken.ThrowIfCancellationRequested();
            scope.Description = scope.Description.Replace(placeholder, currentHostAndPort, StringComparison.OrdinalIgnoreCase);

            foreach (var theme in scope.Themes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                theme.Description =
                    theme.Description.Replace(placeholder, currentHostAndPort, StringComparison.OrdinalIgnoreCase);
            }
        }

        return scopesList;
    }

    public async Task UpdateScope(Guid scopeId, string? scopeNewName, string? scopeNewDescription)
    {
        if (string.IsNullOrWhiteSpace(scopeNewName) && scopeNewDescription == null) return;

        var targetScope = await _context.Scopes.FindAsync(scopeId).ConfigureAwait(false);
        if (targetScope == null) throw new ArgumentException($"Scope with id = {scopeId} is not exists");
        if (targetScope.Deleted) throw new ArgumentException($"Scope with id = {scopeId} is deleted");

        if (!string.IsNullOrWhiteSpace(scopeNewName))
        {
            scopeNewName = scopeNewName.Trim();

            if (await _context.Scopes.AnyAsync(s => s.ScopeName == scopeNewName && s.Id != scopeId))
                throw new ArgumentException($"Scope with name {scopeNewName} already exists");

            targetScope.ScopeName = scopeNewName;
        }

        if (!string.IsNullOrWhiteSpace(scopeNewDescription))
        {
            var placeholder = _appSettingsService.GetHostAndPortPlaceholder();
            var currentHostAndPort = await _appSettingsService.GetHostAndPort();

            targetScope.Description = scopeNewDescription.Trim()
                .Replace(currentHostAndPort, placeholder, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            targetScope.Description = string.Empty;
        }

        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task UpdateTheme(Guid themeId, string? themeNewName, bool? themeActuality, string? themeNewDescription)
    {
        if (string.IsNullOrWhiteSpace(themeNewName) && themeActuality == null && themeNewDescription == null) return;

        var targetTheme = await _context.Themes.FindAsync(themeId);
        if (targetTheme == null) throw new ArgumentException($"Theme with id = {themeId} is not exists");
        if (targetTheme.Deleted) throw new ArgumentException($"Theme with id = {themeId} is deleted");

        if (!string.IsNullOrWhiteSpace(themeNewName))
        {
            themeNewName = themeNewName.Trim();

            if (await _context.Themes
                .AnyAsync(t => t.ScopeId == targetTheme.ScopeId && t.Id != targetTheme.Id && t.ThemeName == themeNewName))
                throw new ArgumentException($"Theme with name {themeNewName} already exists");

            targetTheme.ThemeName = themeNewName;
        }

        if (!string.IsNullOrWhiteSpace(themeNewDescription))
        {
            var placeholder = _appSettingsService.GetHostAndPortPlaceholder();
            var currentHostAndPort = await _appSettingsService.GetHostAndPort();

            targetTheme.Description = themeNewDescription.Trim()
                .Replace(currentHostAndPort, placeholder, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            targetTheme.Description = string.Empty;
        }

        if (themeActuality != null && targetTheme.Actual != themeActuality)
        {
            targetTheme.Actual = themeActuality.Value;
        }

        await _context.SaveChangesAsync().ConfigureAwait(false);
    }
}
