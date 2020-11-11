﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages
{
    public class RecordsViewModel : PageModel
    {
        private readonly IRecordsSearchService _searchRecordsService;
        private readonly IScopesService _scopesSvc;
        public RecordsViewModel(IRecordsSearchService searchRecordsService, IScopesService scopesSvc)
        {
            _searchRecordsService = searchRecordsService;
            _scopesSvc = scopesSvc;
        }

        public IEnumerable<DiaryRecord> Records { get; private set; } = Enumerable.Empty<DiaryRecord>();
        public RecordsFilter Filters { get; private set; } = RecordsFilter.Empty;
        public int RecordsCount { get; private set; }
        public int PagesCount { get; private set; }
        public int CurrenPage { get; private set; }
        public IEnumerable<DiaryScope> AllScopes { get; private set; } = Enumerable.Empty<DiaryScope>();
        public Guid[] SelectedThemes { get; private set; } = Array.Empty<Guid>();
        public bool CombineThemes { get; private set; }
        private const int _pageSize = 30;

        public async Task OnGetSearchAsync(DateTime? fromDate, DateTime? toDate, Guid[] themes, string searchName, bool? combineThemes)
        {
            CombineThemes = combineThemes ?? false;

            Filters = new RecordsFilter
            {
                PageSize = _pageSize,
                RecordDateFrom = fromDate,
                RecordDateTo = toDate,
                RecordNameFilter = searchName?.Trim(),
                PageNo = 0,
                CombineThemes = this.CombineThemes
            };
            if (themes != null && themes.Length > 0)
            {
                Filters.AddThemeId(themes);
                SelectedThemes = themes;
            }
            RecordsCount = await _searchRecordsService.GetRecordsCount(Filters);
            PagesCount = Convert.ToInt32(Math.Ceiling((float)RecordsCount / Filters.PageSize));
            CurrenPage = 0;

            Records = await _searchRecordsService.GetRecordsList(Filters);
            AllScopes = await _scopesSvc.GetScopes();
        }

        public async Task OnGetAsync(DateTime? fromDate, DateTime? toDate, Guid[] themes, string searchName, bool? combineThemes, int recordsCount, int currentPage, int pagesCount, string navTo)
        {
            CombineThemes = combineThemes ?? false;

            if (recordsCount == 0)
            {
                Filters = RecordsFilter.Empty;
                Filters.CombineThemes = this.CombineThemes;
                Filters.PageSize = _pageSize;
                CurrenPage = Filters.PageNo;
                RecordsCount = await _searchRecordsService.GetRecordsCount(Filters);
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
                    "Первая" => 0,
                    "Предыдущая" => currentPage - 1,
                    "Следующая" => currentPage + 1,
                    "Последняя" => PagesCount,
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
                    PageNo = CurrenPage,
                    CombineThemes = this.CombineThemes
                };
                if (themes != null && themes.Length > 0)
                {
                    Filters.AddThemeId(themes);
                    SelectedThemes = themes;
                }
            }

            Records = await _searchRecordsService.GetRecordsList(Filters);
            AllScopes = await _scopesSvc.GetScopes();
        }
    }
}