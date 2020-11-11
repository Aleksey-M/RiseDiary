using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Model
{
    public interface IScopesService
    {
        Task<List<DiaryScope>> GetScopes(bool? themesActuality = null);
        Task<DiaryScope> FetchScopeById(Guid scopeId);
        Task UpdateScopeName(Guid scopeId, string scopeNewName);
        Task<bool> CanDeleteScope(Guid scopeId);
        Task DeleteScope(Guid scopeId);
        Task<Guid> AddScope(string newScopeName);
        Task<Guid> AddTheme(Guid scopeId, string newThemeName, bool actual);
        Task UpdateTheme(Guid themeId, string? themeNewName = null, bool? themeActuality = null);
        Task DeleteTheme(Guid themeId);
    }
}
