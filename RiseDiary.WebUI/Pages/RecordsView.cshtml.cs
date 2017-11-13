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
        private readonly ILogger<ImagesPageModel> _logger;
        public RecordsViewModel(IRepositoriesFactory factory, ILogger<ImagesPageModel> logger)
        {
            _repoFactory = factory;
            _logger = logger;
        }

        public IEnumerable<DiaryRecord> Records { get; set; }
        public RecordsFilter Filters { get; set; } = RecordsFilter.Empty;
        public int RecordsCount { get; set; }
        public int PagesCount { get; set; } = 1;

        //public Dictionary<string, int> SelectedThemes { get; set; }

        private async Task UpdatePageState()
        {
            RecordsCount = await _repoFactory.RecordsRepository.GetFilteredRecordsCount(Filters);
            int pagesCount = Convert.ToInt32( Math.Ceiling((float)RecordsCount / Filters.PageSize));
            if (Filters.PageNo >= pagesCount) Filters.PageNo = pagesCount-1;
            Records = await _repoFactory.RecordsRepository.FetchRecordsListFiltered(Filters);
        }

        public async Task OnGetAsync()
        {
            await UpdatePageState();
        }

        public async Task OnGetSearchAsync(DateTime? fromDate,  DateTime? toDate, string searchName)
        {
            Filters = new RecordsFilter { RecordDateFrom = fromDate, RecordDateTo = toDate, RecordNameFilter = searchName?.Trim() };
            await UpdatePageState();
        }

        public async Task OnGetPrevPageAsync(DateTime? fromDate, DateTime? toDate, string searchName, int pageNo)
        {
            Filters = new RecordsFilter
            {
                RecordDateFrom = fromDate,
                RecordDateTo = toDate,
                RecordNameFilter = searchName?.Trim(),
                PageNo = pageNo > 0 ? pageNo - 1 : 0
            };
            await UpdatePageState();
        }

        public async Task OnGetNextPageAsync(DateTime? fromDate, DateTime? toDate, string searchName, int pageNo)
        {
            Filters = new RecordsFilter
            {
                RecordDateFrom = fromDate,
                RecordDateTo = toDate,
                RecordNameFilter = searchName?.Trim(),
                PageNo = pageNo + 1
            };
            await UpdatePageState();
        }
    }
}