using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages
{
    public class CalendarModel : PageModel
    {
        private readonly DiaryDbContext _context;
        private readonly ILogger<CalendarModel> _logger;
        public CalendarModel(DiaryDbContext context, ILogger<CalendarModel> logger)
        {
            _context = context;
            _logger = logger;
            Filters.PageSize = 1000;
        }

        public List<DiaryRecord> Records { get; set; }
        public RecordsFilter Filters { get; set; } = RecordsFilter.Empty;
        public List<DiaryScope> AllScopes { get; set; }
        public List<DiaryThemeJoined> AllThemes { get; set; }
        public int[] SelectedThemes { get; set; } = new int[0];

        private async Task UpdatePageState()
        {            
            if (Filters.RecordDateFrom == null)
            {
                Filters.RecordDateFrom = new DateTime(DateTime.Now.Year, 01, 01);
                Filters.RecordDateTo = new DateTime(DateTime.Now.Year, 12, 31);                
            }
            AllScopes = await _context.FetchAllScopes();
            AllThemes = await _context.FetchThemesWithScopes();
            Records = await _context.FetchRecordsListFiltered(Filters);
        }

        public async Task OnGetAsync()
        {
            await UpdatePageState();
        }

        public async Task OnGetFilterAsync(int? year, int[] themes)
        {           
            Filters.AddThemeId(themes);
            SelectedThemes = themes;
            if(year != null)
            {
                Filters.RecordDateFrom = new DateTime(year.Value, 01, 01);
                Filters.RecordDateTo = new DateTime(year.Value, 12, 31);
            }
            await UpdatePageState();
        }
    }
}