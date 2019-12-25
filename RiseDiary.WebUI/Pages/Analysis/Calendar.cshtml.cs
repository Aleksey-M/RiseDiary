using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
    public class CalendarModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public CalendarModel(DiaryDbContext context)
        {
            _context = context;
        }

        public IEnumerable<CalendarRecordItem> Records { get; private set; } = Enumerable.Empty<CalendarRecordItem>();
        public int CurrentYear { get; private set; }
        public IEnumerable<DiaryScope> AllScopes { get; private set; } = Enumerable.Empty<DiaryScope>();
#pragma warning disable CA1819 // Properties should not return arrays
        public Guid[] SelectedThemes { get; private set; } = Array.Empty<Guid>();
#pragma warning restore CA1819 // Properties should not return arrays
        public IEnumerable<int> YearsListFiltered { get; private set; } = Enumerable.Empty<int>();

        public async Task OnGetAsync(int? year, Guid[] themes)
        {
            SelectedThemes = themes ?? Array.Empty<Guid>();
            CurrentYear = year ?? DateTime.Now.Year;
            AllScopes = await _context.FetchScopesWithThemes();
            YearsListFiltered = await _context.FetchYearsListFiltered(SelectedThemes);
            Records = await _context.FetchCalendarDates(CurrentYear, SelectedThemes);
        }
    }
}