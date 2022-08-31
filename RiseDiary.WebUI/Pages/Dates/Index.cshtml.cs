﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.Shared;

namespace RiseDiary.WebUI.Pages.Dates
{
    public class IndexModel : PageModel
    {
        private readonly IDatesService _datesService;
        private readonly IAppSettingsService _settingsSvc;

        public IndexModel(IDatesService datesService, IAppSettingsService settingsSvc)
        {
            _datesService = datesService;
            _settingsSvc = settingsSvc;
        }

        public DateOnly Today { get; } = DateOnly.FromDateTime(DateTime.UtcNow);

        public IEnumerable<DateListItem> Dates { get; private set; } = Enumerable.Empty<DateListItem>();

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            var (stringId, _) = await _settingsSvc.GetAppSetting(AppSettingsKey.ImportantDaysScopeId);
            var range = await _settingsSvc.GetAppSettingInt(AppSettingsKey.ImportantDaysDisplayRange);

            if (!Guid.TryParse(stringId, out _) || range == null) return Redirect("~/Dates/Setup");

            try
            {
                Dates = await _datesService.GetDatesFromRange(Today, true, cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }

            return Page();
        }

        public string GetRowStyle(DateListItem date) => (!string.IsNullOrEmpty(date.Themes), date.TransferredDate == Today) switch
        {
            (true, true) => "background-color:#fbc1c1",
            (true, false) => "background-color:#f5face",
            (false, true) => "background-color:#ffeadc",
            (false, false) => "background-color:#cef5d9"
        };
    }
}