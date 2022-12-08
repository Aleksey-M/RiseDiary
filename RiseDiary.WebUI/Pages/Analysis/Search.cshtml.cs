using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.Shared;

namespace RiseDiary.WebUI.Pages.Analysis
{
    public class IndexModel : PageModel
    {
        private readonly IRecordsSearchTextService _recordsSearchTextService;
        private readonly IAppSettingsService _appSettingsService;

        public IndexModel(IRecordsSearchTextService recordsSearchTextService, IAppSettingsService appSettingsService)
        {
            _recordsSearchTextService = recordsSearchTextService;
            _appSettingsService = appSettingsService;
        }

        public IEnumerable<DiaryRecord> Records { get; private set; } = Enumerable.Empty<DiaryRecord>();

        public PagesInfo Pages { get; private set; } = null!;

        public string SearchString { get; private set; } = "";

        public bool Expanded { get; private set; }

        public List<KeyValuePair<string, string?>> SearchParams => new()
        {
            new KeyValuePair<string, string?>(nameof(SearchString), SearchString),
            new KeyValuePair<string, string?>(nameof(Expanded), Expanded.ToString())
        };

        public async Task OnGetAsync(string? searchString, int? pageNo, bool? expanded, CancellationToken cancellationToken)
        {
            pageNo ??= 1;
            Expanded = expanded.HasValue && expanded.Value;
            int pageSize = await _appSettingsService.GetAppSettingInt(AppSettingsKey.RecordsPageSize) ?? 50;

            if (string.IsNullOrWhiteSpace(searchString))
            {
                SearchString = "";
                Pages = PagesInfo.GetPagesInfo(0);
            }
            else
            {
                try
                {
                    SearchString = searchString;
                    int recordsCount = await _recordsSearchTextService.GetRecordsCount(SearchString, cancellationToken);
                    Pages = PagesInfo.GetPagesInfo(recordsCount, pageNo ?? 1, pageSize, 10);
                    Records = await _recordsSearchTextService.GetRecordsList(
                        new RecordsTextFilter
                        {
                            SearchText = SearchString,
                            PageNo = Pages.CurrentPage - 1,
                            PageSize = Pages.PageSize
                        },
                        cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    Pages = PagesInfo.GetPagesInfo(1);
                }
            }
        }
    }
}