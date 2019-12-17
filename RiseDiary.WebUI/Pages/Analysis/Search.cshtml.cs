using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Analysis
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
    public class IndexModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public string SearchString { get; private set; }
        public List<DiaryRecord> Records { get; private set; }
        public int RecordsCount { get; private set; }
        public int PagesCount { get; private set; }
        public int CurrenPage { get; private set; }

        private const int _pageSize = 30;
        private const string _first = "Первая";
        private const string _previous = "Предыдущая";
        private const string _next = "Следующая";
        private const string _last = "Последняя";
        public string First => _first;
        public string Previous => _previous;
        public string Next => _next;
        public string Last => _last;

        public IndexModel(DiaryDbContext context)
        {
            _context = context;
        }

        private string LocalHostAndPort => Request.Scheme + @"://" + Request.Host.Host + ":" + Request.Host.Port;

        public async Task OnGetAsync(int recordsCount, int currentPage, int pagesCount, string navTo, string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                SearchString = string.Empty;
                Records = new List<DiaryRecord>();
                RecordsCount = 0;
                PagesCount = 0;
                CurrenPage = 0;
            }
            else
            {
                SearchString = searchString;
                RecordsCount = await _context.SearchRecordsByTextCount(SearchString);
                if (pagesCount == 0)
                {
                    PagesCount = Convert.ToInt32(Math.Ceiling((float)RecordsCount / _pageSize));
                }
                else
                {
                    PagesCount = pagesCount >= 0 ? pagesCount : 0;
                }
                CurrenPage = currentPage >= 0 ? currentPage : 1;
                CurrenPage = CurrenPage >= PagesCount ? PagesCount - 1 : CurrenPage;

                switch (navTo)
                {
                    case _first: CurrenPage = 0; break;
                    case _previous: CurrenPage = currentPage - 1; break;
                    case _next: CurrenPage = currentPage + 1; break;
                    case _last: CurrenPage = PagesCount; break;
                    default: CurrenPage = 0; break;
                }
                CurrenPage = CurrenPage >= 0 ? CurrenPage : 0;
                CurrenPage = CurrenPage >= PagesCount ? PagesCount - 1 : CurrenPage;

                Records = await _context.SearchRecordsByText(SearchString, CurrenPage * _pageSize, LocalHostAndPort, _pageSize);
            }
        }
    }
}