﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
    public class RetrospectModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public RetrospectModel(DiaryDbContext context)
        {
            _context = context;
        }

        public Dictionary<DiaryRecord, List<Cogitation>> Records { get; private set; } = new Dictionary<DiaryRecord, List<Cogitation>>();
        public RecordsFilter Filters { get; private set; } = RecordsFilter.Empty;
        public int RecordsCount { get; private set; }
        public int PagesCount { get; private set; }
        public int CurrenPage { get; private set; }
        public IEnumerable<DiaryScope> AllScopes { get; private set; } = Enumerable.Empty<DiaryScope>();
#pragma warning disable CA1819 // Properties should not return arrays
        public Guid[] SelectedThemes { get; private set; } = Array.Empty<Guid>();
#pragma warning restore CA1819 // Properties should not return arrays

        private const int _pageSize = 20;
        private const string _first = "Первая";
        private const string _previous = "Предыдущая";
        private const string _next = "Следующая";
        private const string _last = "Последняя";

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

            Records = new Dictionary<DiaryRecord, List<Cogitation>>();
            foreach (var rec in await _context.FetchRecordsListFiltered(Filters, LocalHostAndPort))
            {
                Records.Add(rec, await _context.FetchAllCogitationsOfRecord(rec.Id, LocalHostAndPort));
            }
            AllScopes = await _context.FetchScopesWithThemes();
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

            Records = new Dictionary<DiaryRecord, List<Cogitation>>();
            foreach (var rec in await _context.FetchRecordsListFiltered(Filters, LocalHostAndPort))
            {
                Records.Add(rec, await _context.FetchAllCogitationsOfRecord(rec.Id, LocalHostAndPort));
            }
            AllScopes = await _context.FetchScopesWithThemes();
        }
    }
}
