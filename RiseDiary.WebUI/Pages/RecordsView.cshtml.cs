using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using RiseDiary.WebUI.Data;
using RiseDiary.Model;

namespace RiseDiary.WebUI.Pages
{
    public class RecordsViewModel : PageModel
    {
        private readonly DiaryDbContext _context;
        private readonly ILogger<RecordsViewModel> _logger;
        public RecordsViewModel(DiaryDbContext context, ILogger<RecordsViewModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IEnumerable<DiaryRecord> Records { get; set; }
        public RecordsFilter Filters { get; set; } = RecordsFilter.Empty;
        public int RecordsCount { get; set; }
        public int PagesCount { get; set; } = 1;
        public List<DiaryScope> AllScopes { get; set; }
        public List<DiaryThemeJoined> AllThemes { get; set; }
        public int[] SelectedThemes { get; set; } = new int[0];

        private async Task UpdatePageState()
        {
            RecordsCount = await _context.GetFilteredRecordsCount(Filters);
            int pagesCount = Convert.ToInt32( Math.Ceiling((float)RecordsCount / Filters.PageSize));
            if (Filters.PageNo >= pagesCount) Filters.PageNo = pagesCount-1;
            Records = await _context.FetchRecordsListFiltered(Filters);
            AllScopes = await _context.FetchAllScopes();
            AllThemes = await _context.FetchThemesWithScopes();
        }

        public async Task OnGetAsync()
        {
            await UpdatePageState();
        }

        public async Task OnGetSearchAsync(DateTime? fromDate,  DateTime? toDate, int[] themes, string searchName)
        {            
            Filters = new RecordsFilter { RecordDateFrom = fromDate, RecordDateTo = toDate, RecordNameFilter = searchName?.Trim() };
            if(themes != null && themes.Length > 0)
            {
                Filters.AddThemeId(themes);
                SelectedThemes = themes;
            }
            await UpdatePageState();
        }

        public async Task OnGetPrevPageAsync(DateTime? fromDate, DateTime? toDate, int[] themes, string searchName, int pageNo)
        {
            Filters = new RecordsFilter
            {
                RecordDateFrom = fromDate,
                RecordDateTo = toDate,
                RecordNameFilter = searchName?.Trim(),
                PageNo = pageNo > 0 ? pageNo - 1 : 0
            };
            if (themes != null && themes.Length > 0)
            {
                Filters.AddThemeId(themes);
                SelectedThemes = themes;
            }
            await UpdatePageState();
        }

        public async Task OnGetNextPageAsync(DateTime? fromDate, DateTime? toDate, int[] themes, string searchName, int pageNo)
        {
            Filters = new RecordsFilter
            {
                RecordDateFrom = fromDate,
                RecordDateTo = toDate,
                RecordNameFilter = searchName?.Trim(),
                PageNo = pageNo + 1
            };
            if (themes != null && themes.Length > 0)
            {
                Filters.AddThemeId(themes);
                SelectedThemes = themes;
            }
            await UpdatePageState();
        }
    }
}