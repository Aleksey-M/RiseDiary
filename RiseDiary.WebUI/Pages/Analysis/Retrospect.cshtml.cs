﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages
{
    public class RetrospectModel : PageModel
    {
        private readonly DiaryDbContext _context;
        private readonly ILogger<RetrospectModel> _logger;
        public RetrospectModel(DiaryDbContext context, ILogger<RetrospectModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Dictionary<DiaryRecord, List<Cogitation>> Records { get; private set; }
        public RecordsFilter Filters { get; private set; }
        public int RecordsCount { get; private set; }
        public int PagesCount { get; private set; }
        public int CurrenPage { get; private set; }
        public List<DiaryScope> AllScopes { get; private set; }
        public List<DiaryThemeJoined> AllThemes { get; private set; }
        public int[] SelectedThemes { get; private set; } = new int[0];

        private const int _pageSize = 20;
        private const string _first = "Первая";
        private const string _previous = "Предыдущая";
        private const string _next = "Следующая";
        private const string _last = "Последняя";
        public string First => _first;
        public string Previous => _previous;
        public string Next => _next;
        public string Last => _last;

        public async Task OnGetSearchAsync(DateTime? fromDate, DateTime? toDate, int[] themes, string searchName)
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

            Records = new Dictionary<DiaryRecord, List<Cogitation>>();
            foreach (var rec in await _context.FetchRecordsListFiltered(Filters))
            {
                Records.Add(rec, await _context.FetchAllCogitationsOfRecord(rec.Id));
            }
            AllScopes = await _context.FetchAllScopes();
            AllThemes = await _context.FetchThemesWithScopes();
        }

        public async Task OnGetAsync(DateTime? fromDate, DateTime? toDate, int[] themes, string searchName, int recordsCount, int currentPage, int pagesCount, string navTo)
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

            Records = new Dictionary<DiaryRecord, List<Cogitation>>();
            foreach (var rec in await _context.FetchRecordsListFiltered(Filters))
            {
                Records.Add(rec, await _context.FetchAllCogitationsOfRecord(rec.Id));
            }
            AllScopes = await _context.FetchAllScopes();
            AllThemes = await _context.FetchThemesWithScopes();
        }        
    }
}
