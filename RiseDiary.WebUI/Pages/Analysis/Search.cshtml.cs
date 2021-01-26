using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Analysis
{
    public class IndexModel : PageModel
    {
        private readonly IRecordsSearchTextService _recordsSearchTextService;
        public IndexModel(IRecordsSearchTextService recordsSearchTextService)
        {
            _recordsSearchTextService = recordsSearchTextService;
        }

        public IEnumerable<DiaryRecord> Records { get; private set; } = Enumerable.Empty<DiaryRecord>();
        public PagesInfo Pages { get; private set; } = null!;
        public string SearchString { get; private set; } = "";

        private const int _pageSize = 50;

        public Dictionary<string, string?> SearchParams => new Dictionary<string, string?>
        {
            { nameof(SearchString), SearchString }
        };

        public async Task OnGetAsync(string? searchString, int? pageNo)
        {
            pageNo ??= 1;

            if (string.IsNullOrWhiteSpace(searchString))
            {
                SearchString = "";
                Records = new List<DiaryRecord>();
                Pages = PagesInfo.GetPagesInfo(0);
            }
            else
            {
                SearchString = searchString;
                int recordsCount = await _recordsSearchTextService.GetRecordsCount(SearchString);
                Pages = PagesInfo.GetPagesInfo(recordsCount, pageNo ?? 1, _pageSize, 10);
                Records = await _recordsSearchTextService.GetRecordsList(new RecordsTextFilter { SearchText = SearchString, PageNo = Pages.CurrentPage - 1, PageSize = Pages.PageSize });
            }
        }
    }
}