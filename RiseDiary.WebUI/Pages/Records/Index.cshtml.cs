using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
    public class RecordsViewModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public RecordsViewModel(DiaryDbContext context)
        {
            _context = context;
        }

        public List<DiaryRecord> Records { get; private set; }
        public RecordsFilter Filters { get; private set; }
        public int RecordsCount { get; private set; }
        public int PagesCount { get; private set; }
        public int CurrenPage { get; private set; }
        public List<DiaryScope> AllScopes { get; private set; }
        public List<DiaryThemeJoined> AllThemes { get; private set; }
        public Guid[] SelectedThemes { get; private set; } = Array.Empty<Guid>();

        private const int _pageSize = 30;
        private const string _first = "Первая";
        private const string _previous = "Предыдущая";
        private const string _next = "Следующая";
        private const string _last = "Последняя";
        public string First => _first;
        public string Previous => _previous;
        public string Next => _next;
        public string Last => _last;

        private string LocalHostAndPort => Request.Scheme + @"://" + Request.Host.Host + ":" + Request.Host.Port;

        public async Task OnGetSearchAsync(DateTime? fromDate, DateTime? toDate, Guid[] themes, string searchName)
        {
            Filters = new RecordsFilter
            {
                PageSize = _pageSize,
                RecordDateFrom = fromDate,
                RecordDateTo = toDate,
                RecordNameFilter = searchName?.Trim(),
                PageNo = 0
            };
            if (themes != null && themes.Length > 0)
            {
                Filters.AddThemeId(themes);
                SelectedThemes = themes;
            }
            RecordsCount = await _context.GetFilteredRecordsCount(Filters);
            PagesCount = Convert.ToInt32(Math.Ceiling((float)RecordsCount / Filters.PageSize));
            CurrenPage = 0;

            Records = await _context.FetchRecordsListFiltered(Filters, LocalHostAndPort);
            AllScopes = await _context.FetchAllScopes();
            AllThemes = await _context.FetchThemesWithScopes();
        }

        public async Task OnGetAsync(DateTime? fromDate, DateTime? toDate, Guid[] themes, string searchName, int recordsCount, int currentPage, int pagesCount, string navTo)
        {
            if (recordsCount == 0)
            {
                Filters = RecordsFilter.Empty;
                Filters.PageSize = _pageSize;
                CurrenPage = Filters.PageNo;
                RecordsCount = await _context.GetFilteredRecordsCount(Filters);
                PagesCount = Convert.ToInt32(Math.Ceiling((float)RecordsCount / Filters.PageSize));
            }
            else
            {
                RecordsCount = recordsCount;
                PagesCount = pagesCount >= 0 ? pagesCount : 0;
                CurrenPage = currentPage >= 0 ? currentPage : 1;
                CurrenPage = CurrenPage >= PagesCount ? PagesCount - 1 : CurrenPage;

                CurrenPage = navTo switch
                {
                    _first => 0,
                    _previous => currentPage - 1,
                    _next => currentPage + 1,
                    _last => PagesCount,
                    _ => 0,
                };
                CurrenPage = CurrenPage >= 0 ? CurrenPage : 0;
                CurrenPage = CurrenPage >= PagesCount ? PagesCount - 1 : CurrenPage;

                Filters = new RecordsFilter
                {
                    PageSize = _pageSize,
                    RecordDateFrom = fromDate,
                    RecordDateTo = toDate,
                    RecordNameFilter = searchName?.Trim(),
                    PageNo = CurrenPage
                };
                if (themes != null && themes.Length > 0)
                {
                    Filters.AddThemeId(themes);
                    SelectedThemes = themes;
                }
            }

            Records = await _context.FetchRecordsListFiltered(Filters, LocalHostAndPort);
            AllScopes = await _context.FetchAllScopes();
            AllThemes = await _context.FetchThemesWithScopes();
        }
    }
}