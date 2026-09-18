using Microsoft.EntityFrameworkCore;
using RiseDiary.Common.Core;
using RiseDiary.Data;
using RiseDiary.WebAPI.Application.Scopes.Abstraction;
using RiseDiary.WebAPI.Application.Scopes.Model;
using RiseDiary.WebAPI.Application.Scopes.Services.Commands;

namespace RiseDiary.WebAPI.Application.Scopes.Services;

public class ScopesService : IScopesService
{
    private readonly DiaryDbContext _context;
    private readonly ILogger<ScopesService> _logger;

    public ScopesService(DiaryDbContext context, ILogger<ScopesService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
    }

    public virtual async Task<Result<ApplyOperationResult>> Apply(IScopeOperation operation)
    {
        return (operation) switch
        {
            CreateScope cs => await AddScope(cs),
            UpdateScope us => await UpdateScope(us),
            CreateTheme ct => await AddTheme(ct),
            UpdateTheme ut => await UpdateTheme(ut),
            _ => Result<ApplyOperationResult>.Failure("Операция не поддерживается!")
        };
    }

    private async Task<Result<ApplyOperationResult>> UpdateTheme(UpdateTheme ut)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ut.Name) && ut.Actual == null && ut.Description == null)
            {
                return Result<ApplyOperationResult>.Success(new ApplyOperationResult(ut.ThemeId));
            }

            var targetTheme = await _context.Themes.FindAsync(ut.ThemeId);
            if (targetTheme == null)
            {
                return Result<ApplyOperationResult>.Failure($"Не найдена запись с Id={ut.ThemeId}");
            }

            if (targetTheme.Deleted)
            {
                return Result<ApplyOperationResult>.Failure($"Запись с Id={ut.ThemeId} удалена");
            }

            if (!string.IsNullOrWhiteSpace(ut.Name))
            {
                if (await _context.Themes.AnyAsync(t => t.ScopeId == targetTheme.ScopeId && t.Id != targetTheme.Id && t.ThemeName == ut.Name))
                {
                    return Result<ApplyOperationResult>.Failure("Запись с таким названием уже существует");
                }

                targetTheme.ThemeName = ut.Name;
            }

            targetTheme.Description = ut.Description ?? targetTheme.Description;
            if (ut.Actual != null && targetTheme.Actual != ut.Actual)
            {
                targetTheme.Actual = ut.Actual.Value;
            }

            targetTheme.ModifyDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Result<ApplyOperationResult>.Success(new ApplyOperationResult(ut.ThemeId));
        }
        catch (Exception exc) when (exc is not OperationCanceledException)
        {
            _logger.LogError(exc, "При обновлении темы интересов возникла ошибка");
            return Result<ApplyOperationResult>.Failure("При обновлении темы интересов возникла ошибка");
        }
    }

    private async Task<Result<ApplyOperationResult>> AddTheme(CreateTheme ct)
    {
        try
        {
            var newTheme = new ThemeEntity
            {
                Id = Guid.NewGuid(),
                Actual = ct.Actual,
                ScopeId = ct.ScopeId,
                ThemeName = ct.Name,
                Description = ct.Description ?? string.Empty,
                CreateDate = DateTime.UtcNow,
                ModifyDate = DateTime.UtcNow
            };

            if (string.IsNullOrWhiteSpace(newTheme.ThemeName))
            {
                return Result<ApplyOperationResult>.Failure($"Название новой темы интересов не может быть пустым");
            }

            var scope = await _context.Scopes
                .AsNoTracking()
                .Include(s => s.Themes)
                .SingleOrDefaultAsync(s => s.Id == newTheme.ScopeId);

            if (scope == null)
            {
                return Result<ApplyOperationResult>.Failure($"Область интересов не найдена");
            }

            if (scope.Themes.Any(t => t.ThemeName.Equals(newTheme.ThemeName, StringComparison.OrdinalIgnoreCase)))
            {
                return Result<ApplyOperationResult>.Failure($"Тема с таким названием уже существует");
            }

            await _context.Themes.AddAsync(newTheme);
            await _context.SaveChangesAsync();

            return Result<ApplyOperationResult>.Success(new ApplyOperationResult(newTheme.Id));
        }
        catch (Exception exc) when (exc is not OperationCanceledException)
        {
            _logger.LogError(exc, "При добавлении темы интересов возникла ошибка");
            return Result<ApplyOperationResult>.Failure("При добавлении темы интересов возникла ошибка");
        }
    }

    private async Task<Result<ApplyOperationResult>> UpdateScope(UpdateScope us)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(us.Name) && us.Description == null)
            {
                return Result<ApplyOperationResult>.Success(new ApplyOperationResult(us.ScopeId));
            }

            var targetScope = await _context.Scopes.FindAsync(us.ScopeId);
            if (targetScope == null)
            {
                return Result<ApplyOperationResult>.Failure($"Не найдена запись с Id={us.ScopeId}");
            }

            if (targetScope.Deleted)
            {
                return Result<ApplyOperationResult>.Failure($"Запись с Id={us.ScopeId} удалена");
            }

            if (!string.IsNullOrWhiteSpace(us.Name))
            {
                if (await _context.Scopes.AnyAsync(s => s.ScopeName == us.Name && s.Id != us.ScopeId))
                {
                    return Result<ApplyOperationResult>.Failure("Запись с таким названием уже существует");
                }

                targetScope.ScopeName = us.Name;
            }

            targetScope.Description = us.Description ?? targetScope.Description;
            targetScope.ModifyDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Result<ApplyOperationResult>.Success(new ApplyOperationResult(us.ScopeId));
        }
        catch (Exception exc) when (exc is not OperationCanceledException)
        {
            _logger.LogError(exc, "При обновлении области интересов возникла ошибка");
            return Result<ApplyOperationResult>.Failure("При обновлении области интересов возникла ошибка");
        }
    }

    private async Task<Result<ApplyOperationResult>> AddScope(CreateScope cs)
    {
        try
        {
            var scope = new ScopeEntity
            {
                Id = Guid.NewGuid(),
                ScopeName = cs.Name,
                Description = cs.Description ?? string.Empty,
                CreateDate = DateTime.UtcNow,
                ModifyDate = DateTime.UtcNow
            };

            if (string.IsNullOrWhiteSpace(scope.ScopeName))
            {
                return Result<ApplyOperationResult>.Failure("Название новой области интересов не может быть пустым");
            }

            if (await _context.Scopes.AnyAsync(s => s.ScopeName == scope.ScopeName))
            {
                return Result<ApplyOperationResult>.Failure("Такая область интересов уже есть");
            }

            _context.Scopes.Add(scope);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Добавлена новая область интересов: {@scope}", scope);
            return Result<ApplyOperationResult>.Success(new ApplyOperationResult(scope.Id));
        }
        catch (Exception exc) when (exc is not OperationCanceledException)
        {
            _logger.LogError(exc, "При добавлении новой области интересов возникла ошибка");
            return Result<ApplyOperationResult>.Failure("При добавлении новой области интересов возникла ошибка");
        }
    }

    public virtual async Task<Result> DeleteScope(Guid scopeId)
    {
        try
        {
            var scope = await _context.Scopes
                .Include(s => s.Themes)
                .ThenInclude(t => t.RecordsRefs)
                .SingleOrDefaultAsync(s => s.Id == scopeId);

            if (scope != null)
            {
                _context.Scopes.Remove(scope);
                await _context.SaveChangesAsync();
            }

            return Result.Success();
        }
        catch (Exception exc) when (exc is not OperationCanceledException)
        {
            _logger.LogError(exc, "Ошибка при удалении сферы интересов");
            return Result.Failure("Ошибка при удалении сферы интересов");
        }
    }

    public virtual async Task<Result> DeleteTheme(Guid themeId)
    {
        try
        {
            var theme = await _context.Themes
            .Include(t => t.RecordsRefs)
            .SingleOrDefaultAsync(t => t.Id == themeId);

            if (theme != null)
            {
                _context.Themes.Remove(theme);
                await _context.SaveChangesAsync();
            }

            return Result.Success();
        }
        catch (Exception exc) when (exc is not OperationCanceledException)
        {
            _logger.LogError(exc, "Ошибка при удалении темы");
            return Result.Failure("Ошибка при удалении темы");
        }
    }

    public virtual async Task<Result<ScopeEntity[]>> GetScopes(CancellationToken cancellationToken)
    {
        try
        {
            var scopesList = await _context.Scopes
            .Include(s => s.Themes)
            .ThenInclude(t => t.RecordsRefs)
            .AsNoTracking()
            .OrderBy(s => s.ScopeName)
            .ToListAsync(cancellationToken);

            return Result<ScopeEntity[]>.Success([.. scopesList]);
        }
        catch (Exception exc) when (exc is not OperationCanceledException)
        {
            _logger.LogError(exc, "Ошибка при получении сфер и тем инетерсов");
            return Result<ScopeEntity[]>.Failure("Ошибка при получении сфер и тем инетерсов");
        }
    }
}
