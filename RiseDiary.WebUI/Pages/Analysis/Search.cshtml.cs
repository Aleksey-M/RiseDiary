using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Analysis
{
    public class IndexModel : PageModel
    {
        private readonly IRecordsSearchTextService _recordsSearchTextService;
        public string SearchString { get; private set; } = "";
        public IEnumerable<DiaryRecord> Records { get; private set; } = Enumerable.Empty<DiaryRecord>();
        public int RecordsCount { get; private set; }
        public int PagesCount { get; private set; }
        public int CurrenPage { get; private set; }

        private const int _pageSize = 30;
        private const string _first = "Первая";
        private const string _previous = "Предыдущая";
        private const string _next = "Следующая";
        private const string _last = "Последняя";

        public IndexModel(IRecordsSearchTextService recordsSearchTextService)
        {
            _recordsSearchTextService = recordsSearchTextService;
        }

        public async Task OnGetAsync(int currentPage, int pagesCount, string navTo, string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                SearchString = "";
                Records = new List<DiaryRecord>();
                RecordsCount = 0;
                PagesCount = 0;
                CurrenPage = 0;
            }
            else
            {
                SearchString = searchString;
                RecordsCount = await _recordsSearchTextService.GetRecordsCount(SearchString);

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

                var pInfo = PagesInfo.GetPagesInfo(RecordsCount, CurrenPage, _pageSize);
                Records = await _recordsSearchTextService.GetRecordsList(new RecordsTextFilter { SearchText = SearchString, PageNo = pInfo.CurrentPage-1, PageSize = pInfo.PageSize });
            }
        }
    }
}