using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Dates
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
    public class SetupModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public List<DiaryScope> Scopes;
        public Guid SelectedScopeId;
        public int DaysDisplayRange;
        public string Message;

        public SetupModel(DiaryDbContext context)
        {
            _context = context;
        }

        public async Task UpdateViewModel()
        {
            Scopes = await _context.FetchScopesWithThemes();
            var setting = await _context.GetAppSetting(AppSettingsKeys.DatesScopeId);
            if (setting != null && Guid.TryParse(setting, out Guid id))
            {
                SelectedScopeId = id;
            }
            else
                SelectedScopeId = Guid.Empty;
            DaysDisplayRange = await _context.GetAppSettingInt(AppSettingsKeys.DatesDisplayRange) ?? 20;
        }

        public async Task OnGetAsync()
        {
            await UpdateViewModel();
        }

        public async Task OnPostAsync(Guid scopeId, int displayRange)
        {
            if (scopeId  == default)
            {
                Message = "Некорректная область";
                return;
            }

            if (displayRange <= 0 || displayRange >= 35)
            {
                Message = "Некорректное значение диапазона";
                return;
            }

            await _context.UpdateAppSetting(AppSettingsKeys.DatesScopeId, scopeId.ToString());
            await _context.UpdateAppSetting(AppSettingsKeys.DatesDisplayRange, displayRange.ToString());
            Message = "Данные обновлены";
            await UpdateViewModel();
        }

    }
}