using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RiseDiary.Domain.Model;
using RiseDiary.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages
{
    public class RetrospectModel : PageModel
    {
        private readonly IRepositoriesFactory _repoFactory;
        private readonly ILogger<RetrospectModel> _logger;
        public RetrospectModel(IRepositoriesFactory factory, ILogger<RetrospectModel> logger)
        {
            _repoFactory = factory;
            _logger = logger;
        }

        public Dictionary<DiaryRecord, List<Cogitation>> Records { get; set; }
        public RecordsFilter Filters { get; set; } = RecordsFilter.Empty;
        public int RecordsCount { get; set; }
        public int PagesCount { get; set; } = 1;
        public List<DiaryArea> AllAreas { get; set; }
        public List<DiaryRecordTypeJoined> AllTypes { get; set; }
        public int[] SelectedThemes { get; set; } = new int[0];

        private async Task UpdatePageState()
        {
            Filters.PageSize = 10;
            RecordsCount = await _repoFactory.RecordsRepository.GetFilteredRecordsCount(Filters);
            int pagesCount = Convert.ToInt32(Math.Ceiling((float)RecordsCount / Filters.PageSize));
            if (Filters.PageNo >= pagesCount) Filters.PageNo = pagesCount - 1;
            Records = new Dictionary<DiaryRecord, List<Cogitation>>();
            foreach(var rec in await _repoFactory.RecordsRepository.FetchRecordsListFiltered(Filters))
            {
                Records.Add(rec, await _repoFactory.CogitationRepository.FetchAllCogitationsOfRecord(rec.RecordId));
            }            
            AllAreas = await _repoFactory.AreasRepository.FetchAllAreas();
            AllTypes = await _repoFactory.RecordTypesRepository.FetchRecordTypesWithAreas();
        }

        public async Task OnGetAsync()
        {
            await UpdatePageState();
        }

        public async Task OnGetSearchAsync(DateTime? fromDate, DateTime? toDate, int[] themes, string searchName)
        {
            Filters = new RecordsFilter { RecordDateFrom = fromDate, RecordDateTo = toDate, RecordNameFilter = searchName?.Trim() };
            if (themes != null && themes.Length > 0)
            {
                Filters.AddRecordTypeId(themes);
                SelectedThemes = themes;
            }
            await UpdatePageState();
        }

        public async Task OnGetPrevPageAsync(DateTime? fromDate, DateTime? toDate, int[] themes, string searchName, int pageNo)
        {
            Filters = new RecordsFilter
            {
                RecordDateFrom = fromDate,
                RecordDateTo = toDate,
                RecordNameFilter = searchName?.Trim(),
                PageNo = pageNo > 0 ? pageNo - 1 : 0
            };
            if (themes != null && themes.Length > 0)
            {
                Filters.AddRecordTypeId(themes);
                SelectedThemes = themes;
            }
            await UpdatePageState();
        }

        public async Task OnGetNextPageAsync(DateTime? fromDate, DateTime? toDate, int[] themes, string searchName, int pageNo)
        {
            Filters = new RecordsFilter
            {
                RecordDateFrom = fromDate,
                RecordDateTo = toDate,
                RecordNameFilter = searchName?.Trim(),
                PageNo = pageNo + 1
            };
            if (themes != null && themes.Length > 0)
            {
                Filters.AddRecordTypeId(themes);
                SelectedThemes = themes;
            }
            await UpdatePageState();
        }
    }
}
