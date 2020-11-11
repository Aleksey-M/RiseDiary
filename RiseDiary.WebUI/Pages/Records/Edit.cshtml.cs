using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages
{
    public class RecordEditModel : PageModel
    {
        private readonly IRecordsService _recordsService;
        private readonly IScopesService _scopesSvc;
        private readonly IRecordsThemesService _recordThemesSvc;

        public RecordEditModel(IRecordsService recordsService, IScopesService scopesSvc, IRecordsThemesService recordThemesSvc)
        {
            _recordsService = recordsService;
            _scopesSvc = scopesSvc;
            _recordThemesSvc = recordThemesSvc;
        }

        public Guid? RecordId { get; set; }
        public DateTime? RecordDate { get; set; }
        public DateTime? RecordCreateDate { get; set; }
        public DateTime? RecordModifyDate { get; set; }
        public string? RecordName { get; set; }
        public string? RecordText { get; set; }
        public IEnumerable<Guid> RecordThemesIds { get; set; } = Enumerable.Empty<Guid>();
        public IEnumerable<DiaryScope> Scopes { get; private set; } = Enumerable.Empty<DiaryScope>();

        public IEnumerable<(Guid themeId, string themeName, string scopeName)> ActualOrSelectedThemes => Scopes
            .SelectMany(s => s.Themes)
            .Where(t => RecordThemesIds.Contains(t.Id) || t.Actual)
            .Select(t => (themeId: t.Id, themeName: t.ThemeName, scopeName: t.Scope?.ScopeName ?? ""))
            .OrderBy(t => t.scopeName)
            .ThenBy(t => t.themeName);


        public IEnumerable<(Guid themeId, string themeName, string scopeName)> NotActualThemes => Scopes
            .SelectMany(s => s.Themes)
            .Where(t => !RecordThemesIds.Contains(t.Id) && !t.Actual)
            .Select(t => (themeId: t.Id, themeName: t.ThemeName, scopeName: t.Scope?.ScopeName ?? ""))
            .OrderBy(t => t.scopeName)
            .ThenBy(t => t.themeName);

        private async Task UpdatePageState()
        {
            Scopes = await _scopesSvc.GetScopes();
            RecordDate = DateTime.Now.Date;
            RecordThemesIds = new List<Guid>();
            if (RecordId != null)
            {
                var rec = await _recordsService.FetchRecordById(RecordId.Value);
                if (rec != null)
                {
                    RecordDate = rec.Date;
                    RecordCreateDate = rec.CreateDate;
                    RecordModifyDate = rec.ModifyDate;
                    RecordName = rec.Name;
                    RecordText = rec.Text;
                    RecordThemesIds = rec.ThemesRefs.Select(tr => tr.Theme?.Id ?? default).ToList();
                }
            }
        }

        public async Task OnGetAsync(Guid? recordId)
        {
            RecordId = recordId;
            await UpdatePageState();
        }

        public async Task OnPostSaveRecordAsync(Guid? recordId, DateTime recordDate, string recordName, string recordText, Guid[] themeId)
        {
            themeId ??= Array.Empty<Guid>();
            if (recordId == null)
            {
                Guid newRecordId = await _recordsService.AddRecord(recordDate, recordName?.Trim() ?? "", recordText?.Trim() ?? "");
                await _recordThemesSvc.AddRecordTheme(newRecordId, themeId);

                RecordId = newRecordId;
            }
            else
            {
                var record = await _recordsService.FetchRecordById(recordId.Value);
                if (record != null)
                {
                    await _recordsService.UpdateRecord(recordId.Value, recordDate, recordName?.Trim() ?? "", recordText?.Trim() ?? "");

                    var recThemesIds = record.ThemesRefs.Select(tr => tr.Theme?.Id ?? default).ToList();
                    await _recordThemesSvc.RemoveRecordTheme(record.Id, recThemesIds.Except(themeId));
                    await _recordThemesSvc.AddRecordTheme(record.Id, themeId.Except(recThemesIds));

                    RecordId = record.Id;
                }
            }
            await UpdatePageState();
        }

        public async Task<IActionResult> OnPostDeleteRecordAsync(Guid recordId)
        {
            await _recordsService.DeleteRecord(recordId);
            return Redirect("~/Records");
        }
    }
}