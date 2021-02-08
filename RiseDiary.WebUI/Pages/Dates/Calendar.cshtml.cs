using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Dates
{
    public class CalendarModel : PageModel
    {
        private readonly IDatesService _datesService;
        private readonly IAppSettingsService _settingsSvc;

        public CalendarModel(IDatesService datesService, IAppSettingsService settingsSvc)
        {
            _datesService = datesService;
            _settingsSvc = settingsSvc;
        }

        public IEnumerable<DateListItem> Dates { get; private set; } = Enumerable.Empty<DateListItem>();
        public DateTime Today { get; } = DateTime.Now.Date;

        public async Task<IActionResult> OnGetAsync()
        {
            var (stringId, _) = await _settingsSvc.GetAppSetting(AppSettingsKey.ImportantDaysScopeId);
            var range = await _settingsSvc.GetAppSettingInt(AppSettingsKey.ImportantDaysDisplayRange);

            if (!Guid.TryParse(stringId, out _) || range == null) return Redirect("~/Dates/Setup");

            Dates = await _datesService.GetAllDates(DateTime.Now.Date);
            return Page();
        }

        public string RecordsJsObjectsList
        {
            get
            {
                int count = Dates.Count();
                if (count == 0) return "[]";

                var sb = new StringBuilder("[");

                foreach (var d in Dates)
                {
                    sb.Append("{id:\"").Append(d.Id).Append("\",");
                    sb.Append("name:\"").Append(d.Themes.Replace("\"", "\\\"")).Append(": ").Append(d.Name.Replace("\"", "\\\"")).Append("\",");
                    sb.Append("startDate: new Date(").Append(d.TransferredDate.Year).Append(',')
                        .Append(d.TransferredDate.Month - 1).Append(',')
                        .Append(d.TransferredDate.Day).Append(",0,0,0),");
                    sb.Append("endDate: new Date(").Append(d.TransferredDate.Year).Append(',')
                        .Append(d.TransferredDate.Month - 1).Append(',')
                        .Append(d.TransferredDate.Day).Append(",0,0,0),");
                    sb.Append("},");
                }

                sb.Remove(sb.Length - 1, 1);

                sb.Append(']');

                return sb.ToString();
            }
        }
    }
}