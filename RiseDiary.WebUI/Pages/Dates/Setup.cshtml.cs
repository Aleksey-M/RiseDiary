using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Pages.Dates
{
    public class SetupModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public List<DiaryScope> Scopes;
        public int SelectedScopeId;
        public int DaysDisplayRange;
        public string Message;

        public SetupModel(DiaryDbContext context)
        {
            _context = context;
        }

        public async Task UpdateViewModel()
        {
            Scopes = await _context.FetchAllScopes();
            SelectedScopeId = await _context.GetAppSettingInt(AppSettingsKeys.DatesScopeId) ?? 0;
            DaysDisplayRange = await _context.GetAppSettingInt(AppSettingsKeys.DatesDisplayRange) ?? 20;
        }

        public async Task OnGetAsync()
        {
            await UpdateViewModel();
        }

        public async Task OnPostAsync(int scopeId, int displayRange)
        {            
            if (scopeId < 0)
            {
                Message = "Некорректная область";
                return;
            }
            if(displayRange <= 0)
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