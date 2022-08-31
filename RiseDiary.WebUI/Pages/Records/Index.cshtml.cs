using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.Shared;

namespace RiseDiary.WebUI.Pages
{
    public class RecordsViewModel : PageModel
    {
        private readonly IRecordsSearchService _searchRecordsService;
        private readonly IScopesService _scopesSvc;
        private readonly IAppSettingsService _appSettingsService;

        public RecordsViewModel(IRecordsSearchService searchRecordsService,
            IScopesService scopesSvc, IAppSettingsService appSettingsService)
        {
            _searchRecordsService = searchRecordsService;
            _scopesSvc = scopesSvc;
            _appSettingsService = appSettingsService;
        }

        public IEnumerable<DiaryRecord> Records { get; private set; } = Enumerable.Empty<DiaryRecord>();

        public RecordsFilter Filters { get; private set; } = RecordsFilter.Empty;

        public PagesInfo Pages { get; private set; } = null!;

        public IEnumerable<DiaryScope> AllScopes { get; private set; } = Enumerable.Empty<DiaryScope>();

        public Guid[] SelectedThemes { get; private set; } = Array.Empty<Guid>();

        public bool Expanded { get; private set; }

        public bool CombineThemes { get; private set; }

        public IEnumerable<KeyValuePair<string, string?>> GetParams() => Filters.GetValuesList()
            .Append(new KeyValuePair<string, string?>(nameof(Expanded), Expanded.ToString()))
            .ToList();

        public async Task OnGetAsync(
            DateTime? fromDate,
            DateTime? toDate,
            Guid[] themes,
            string? filterName,
            bool? combineThemes,
            int? pageNo,
            bool? expanded,
            CancellationToken cancellationToken)
        {
            CombineThemes = combineThemes ?? false;
            Expanded = expanded ?? false;
            int pageSize = await _appSettingsService.GetAppSettingInt(AppSettingsKey.RecordsPageSize) ?? 25;

            Filters = new RecordsFilter
            {
                PageSize = pageSize,
                FromDate = fromDate == null ? null : DateOnly.FromDateTime(fromDate.Value),
                ToDate = toDate == null ? null : DateOnly.FromDateTime(toDate.Value),
                FilterName = filterName?.Trim(),
                PageNo = (pageNo ?? 1) - 1,
                CombineThemes = CombineThemes
            };

            if (themes != null && themes.Length > 0)
            {
                Filters.AddThemeId(themes);
                SelectedThemes = themes;
            }

            try
            {
                var recordsCount = await _searchRecordsService.GetRecordsCount(Filters, cancellationToken);
                Pages = PagesInfo.GetPagesInfo(recordsCount, Filters.PageNo + 1, pageSize, 10);
                Records = await _searchRecordsService.GetRecordsList(Filters, cancellationToken);
                AllScopes = await _scopesSvc.GetScopes(cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Pages = PagesInfo.GetPagesInfo(1);
            }
        }
    }
}