using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;

namespace RiseDiary.WebUI.Pages.Analysis
{
    public class ThisDayModel : PageModel
    {
        private readonly IRecordsSearchService _recordsSearchService;

        public ThisDayModel(IRecordsSearchService recordsSearchService)
        {
            _recordsSearchService = recordsSearchService;
        }

        public IEnumerable<DiaryRecord> Records { get; private set; } = Enumerable.Empty<DiaryRecord>();

        public string ThisDay => DateTime.UtcNow.ToString("m", CultureInfo.GetCultureInfo("ru-RU"));

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            try
            {
                var today = DateTime.UtcNow;
                Records = await _recordsSearchService.GetThisDayRecords(month: today.Month, day: today.Day, cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
