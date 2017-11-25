using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using RiseDiary.Domain.Model;
using System.Collections.Generic;
using System;

namespace RiseDiary.WebUI.Pages
{
    public class RecordsViewModel : PageModel
    {
        private readonly IRepositoriesFactory _repoFactory;
        private readonly ILogger<RecordsViewModel> _logger;
        public RecordsViewModel(IRepositoriesFactory factory, ILogger<RecordsViewModel> logger)
        {
            _repoFactory = factory;
            _logger = logger;
        }

        public IEnumerable<DiaryRecord> Records { get; set; }
        public RecordsFilter Filters { get; set; } = RecordsFilter.Empty;
        public int RecordsCount { get; set; }
        public int PagesCount { get; set; } = 1;
        public List<DiaryArea> AllAreas { get; set; }
        public List<DiaryRecordTypeJoined> AllTypes { get; set; }
        public int[] SelectedThemes { get; set; } = new int[0];

        private async Task UpdatePageState()
        {
            RecordsCount = await _repoFactory.RecordsRepository.GetFilteredRecordsCount(Filters);
            int pagesCount = Convert.ToInt32( Math.Ceiling((float)RecordsCount / Filters.PageSize));
            if (Filters.PageNo >= pagesCount) Filters.PageNo = pagesCount-1;
            Records = await _repoFactory.RecordsRepository.FetchRecordsListFiltered(Filters);
            AllAreas = await _repoFactory.AreasRepository.FetchAllAreas();
            AllTypes = await _repoFactory.RecordTypesRepository.FetchRecordTypesWithAreas();
        }

        public async Task OnGetAsync()
        {
            await UpdatePageState();
        }

        public async Task OnGetSearchAsync(DateTime? fromDate,  DateTime? toDate, int[] themes, string searchName)
        {
            _logger.LogInformation(themes?.Length.ToString() ?? "Empty");
            Filters = new RecordsFilter { RecordDateFrom = fromDate, RecordDateTo = toDate, RecordNameFilter = searchName?.Trim() };
            if(themes != null && themes.Length > 0)
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