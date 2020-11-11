using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages
{
    public class CalendarModel : PageModel
    {
        private readonly ICalendarService _calendarServices;
        private readonly IScopesService _scopeSvc;
        public CalendarModel(ICalendarService calendarServices, IScopesService scopeSvc)
        {
            _calendarServices = calendarServices;
            _scopeSvc = scopeSvc;
        }

        public IEnumerable<CalendarItem> Records { get; private set; } = Enumerable.Empty<CalendarItem>();
        public int CurrentYear { get; private set; }
        public IEnumerable<DiaryScope> AllScopes { get; private set; } = Enumerable.Empty<DiaryScope>();
        public IEnumerable<Guid> SelectedThemes { get; private set; } = Enumerable.Empty<Guid>();
        public IEnumerable<int> YearsListFiltered { get; private set; } = Enumerable.Empty<int>();
        public bool CombineThemes { get; private set; }

        public async Task OnGetAsync(int? year, Guid[] themes, bool? combineThemes)
        {
            CombineThemes = combineThemes ?? false;
            SelectedThemes = themes ?? Enumerable.Empty<Guid>();
            CurrentYear = year ?? DateTime.Now.Year;
            AllScopes = await _scopeSvc.GetScopes();
            YearsListFiltered = await _calendarServices.GetYears(SelectedThemes, CombineThemes);
            Records = await _calendarServices.GetCalendarItems(CurrentYear, SelectedThemes, CombineThemes);
        }
    }
}