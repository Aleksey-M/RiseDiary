using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Dates
{
    public class SetupModel : PageModel
    {
        private readonly IScopesService _scopesSvc;
        private readonly IAppSettingsService _settingsSvc;

        public SetupModel(IScopesService scopesSvc, IAppSettingsService settingsSvc)
        {
            _scopesSvc = scopesSvc;
            _settingsSvc = settingsSvc;
        }

        public IEnumerable<DiaryScope> Scopes { get; private set; } = Enumerable.Empty<DiaryScope>();
        public Guid SelectedScopeId { get; private set; }
        public int DaysDisplayRange { get; private set; }
        public string Message { get; private set; } = "Выберите источник дат и периоды отображения";
        public string? ErrorMessage { get; private set; }

        public async Task UpdateViewModel()
        {
            Scopes = await _scopesSvc.GetScopes();
            var (setting, _) = await _settingsSvc.GetAppSetting(AppSettingsKey.ImportantDaysScopeId);
            if (setting != null && Guid.TryParse(setting, out Guid id))
            {
                SelectedScopeId = id;
            }
            else
            {
                SelectedScopeId = Guid.Empty;
            }
            DaysDisplayRange = await _settingsSvc.GetAppSettingInt(AppSettingsKey.ImportantDaysDisplayRange) ?? 20;
        }

        public async Task OnGetAsync()
        {
            await UpdateViewModel();
        }

        public async Task OnPostAsync(Guid scopeId, int displayRange)
        {
            bool validInput = true;

            if (scopeId == default)
            {
                ErrorMessage = "Некорректная область";
                validInput = false;
            }

            if (displayRange <= 0 || displayRange >= 35)
            {
                ErrorMessage = "Некорректное значение диапазона";
                validInput = false;
            }

            if (validInput)
            {
                await _settingsSvc.UpdateAppSetting(AppSettingsKey.ImportantDaysScopeId, scopeId.ToString());
                await _settingsSvc.UpdateAppSetting(AppSettingsKey.ImportantDaysDisplayRange, displayRange.ToString(CultureInfo.InvariantCulture));
                Message = "Данные обновлены";
                ErrorMessage = null;
            }

            await UpdateViewModel();
        }

    }
}