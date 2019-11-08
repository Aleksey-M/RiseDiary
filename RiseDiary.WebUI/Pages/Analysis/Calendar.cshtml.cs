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
        public CalendarModel(DiaryDbContext context, ILogger<CalendarModel> logger)
        {
            _context = context;
        }

        public List<CalendarRecordItem> Records { get; private set; }
        public int CurrentYear { get; private set; }
        public List<DiaryScope> AllScopes { get; private set; }
        public List<DiaryThemeJoined> AllThemes { get; private set; }
        public Guid[] SelectedThemes { get; private set; } = new Guid[0];
        public List<int> YearsListFiltered { get; private set; } = new List<int>();

        public async Task OnGetAsync(int? year, Guid[] themes)
        {
            SelectedThemes = themes ?? Array.Empty<Guid>();
            CurrentYear = year ?? DateTime.Now.Year;
            AllScopes = await _context.FetchAllScopes();
            AllThemes = await _context.FetchThemesWithScopes();
            YearsListFiltered = await _context.FetchYearsListFiltered(SelectedThemes);
            Records = await _context.FetchCalendarDates(CurrentYear, SelectedThemes);
        }
    }
}