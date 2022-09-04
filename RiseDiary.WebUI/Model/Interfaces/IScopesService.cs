using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RiseDiary.Model
{
    public interface IScopesService
    {
        Task<List<DiaryScope>> GetScopes(bool? themesActuality = null, CancellationToken cancellationToken = default);

        Task<DiaryScope?> FetchScopeById(Guid scopeId, CancellationToken cancellationToken = default);

        /// <summary> Обновление полей, параметры для которых не равны null </summary>
        Task UpdateScope(Guid scopeId, string? scopeNewName = null, string? scopeNewDescription = null);

        Task<bool> CanDeleteScope(Guid scopeId);

        Task DeleteScope(Guid scopeId);

        Task<Guid> AddScope(string newScopeName, string newScopeDescription = "");

        Task<Guid> AddTheme(Guid scopeId, string newThemeName, bool actual, string newThemeDescription = "");

        /// <summary> Обновление полей, параметры для которых не равны null </summary>
        Task UpdateTheme(Guid themeId, string? themeNewName = null, bool? themeActuality = null, string? themeNewDescription = null);

        Task DeleteTheme(Guid themeId);
    }
}
