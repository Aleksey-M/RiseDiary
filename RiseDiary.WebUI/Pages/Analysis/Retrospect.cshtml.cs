using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages
{
    public class RetrospectModel : PageModel
    {
        private readonly IRecordsSearchService _searchRecordsService;
        private readonly IScopesService _scopesSvc;
        public RetrospectModel(IRecordsSearchService searchRecordsService, IScopesService scopesSvc)
        {
            _searchRecordsService = searchRecordsService;
            _scopesSvc = scopesSvc;
        }

        public IEnumerable<DiaryRecord> Records { get; private set; } = Enumerable.Empty<DiaryRecord>();
        public RecordsFilter Filters { get; private set; } = RecordsFilter.Empty;
        public PagesInfo Pages { get; private set; } = null!;
        public IEnumerable<DiaryScope> AllScopes { get; private set; } = Enumerable.Empty<DiaryScope>();
        public Guid[] SelectedThemes { get; private set; } = Array.Empty<Guid>();

        private const int _pageSize = 20;
        public bool CombineThemes { get; private set; }

        public async Task OnGetAsync(DateTime? fromDate, DateTime? toDate, Guid[] themes, string? filterName, bool? combineThemes, int? pageNo)
        {
            CombineThemes = combineThemes ?? false;

            Filters = new RecordsFilter
            {
                PageSize = _pageSize,
                FromDate = fromDate,
                ToDate = toDate,
                FilterName = filterName?.Trim(),
                PageNo = (pageNo ?? 1) - 1,
                CombineThemes = CombineThemes
            };

            if (themes != null && themes.Length > 0)
            {
                Filters.AddThemeId(themes);
                SelectedThemes = themes;
            }

            var recordsCount = await _searchRecordsService.GetRecordsCount(Filters);
            Pages = PagesInfo.GetPagesInfo(recordsCount, Filters.PageNo + 1, _pageSize, 10);
            Records = await _searchRecordsService.GetRecordsList(Filters);
            AllScopes = await _scopesSvc.GetScopes();
        }
    }
}
