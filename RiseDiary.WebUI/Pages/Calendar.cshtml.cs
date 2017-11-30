using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RiseDiary.Domain.Model;
using RiseDiary.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages
{
    public class CalendarModel : PageModel
    {
        private readonly IRepositoriesFactory _repoFactory;
        private readonly ILogger<CalendarModel> _logger;
        public CalendarModel(IRepositoriesFactory factory, ILogger<CalendarModel> logger)
        {
            _repoFactory = factory;
            _logger = logger;
            Filters.PageSize = 1000;
        }

        public List<DiaryRecord> Records { get; set; }
        public RecordsFilter Filters { get; set; } = RecordsFilter.Empty;
        public List<DiaryArea> AllAreas { get; set; }
        public List<DiaryRecordTypeJoined> AllTypes { get; set; }
        public int[] SelectedThemes { get; set; } = new int[0];

        private async Task UpdatePageState()
        {            
            if (Filters.RecordDateFrom == null)
            {
                Filters.RecordDateFrom = new DateTime(DateTime.Now.Year, 01, 01);
                Filters.RecordDateTo = new DateTime(DateTime.Now.Year, 12, 31);
                _logger.LogWarning("Default Year");
            }
            AllAreas = await _repoFactory.AreasRepository.FetchAllAreas();
            AllTypes = await _repoFactory.RecordTypesRepository.FetchRecordTypesWithAreas();
            Records = await _repoFactory.RecordsRepository.FetchRecordsListFiltered(Filters);
        }

        public async Task OnGetAsync()
        {
            await UpdatePageState();
        }

        public async Task OnGetFilterAsync(int? year, int[] themes)
        {
            _logger.LogWarning($"Year = {year}");
            Filters.AddRecordTypeId(themes);
            SelectedThemes = themes;
            if(year != null)
            {
                Filters.RecordDateFrom = new DateTime(year.Value, 01, 01);
                Filters.RecordDateTo = new DateTime(year.Value, 12, 31);
            }
            await UpdatePageState();
        }
    }
}