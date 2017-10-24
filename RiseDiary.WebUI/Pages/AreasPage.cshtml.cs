using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Domain.Model;
using RiseDiary.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace RiseDiary.WebUI.Pages
{
    public class AreasPageModel : PageModel
    {
        private readonly IRepositoriesFactory _repoFactory;
        private readonly ILogger<AreasPageModel> _logger;
        public AreasPageModel(IRepositoriesFactory factory, ILogger<AreasPageModel> logger)
        {
            _repoFactory = factory;
            _logger = logger;            
        }

        private IEnumerable<DiaryArea> _areas = new List<DiaryArea>();
        public IEnumerable<DiaryArea> Areas { get => _areas; }
        private IEnumerable<DiaryRecordTypeJoined> _recTypes = new List<DiaryRecordTypeJoined>();
        public IEnumerable<DiaryRecordTypeJoined> RecordTypes { get => _recTypes; }

        public async Task OnGetAsync()
        {
            await UpdatePageState();
        }
               
        private async Task UpdatePageState()
        {
            _areas = await _repoFactory.AreasRepository.FetchAllAreas();
            _recTypes = await _repoFactory.RecordTypesRepository.FetchRecordTypesWithAreas();
        }

        public async Task OnPostAddAreaAsync(string newAreaName)
        {
            if (!string.IsNullOrWhiteSpace(newAreaName))
            {
                await _repoFactory.AreasRepository.AddArea(newAreaName);
            }
            else
            {
                ModelState.AddModelError("Theme Name", "Название темы не введено");
            }
            await UpdatePageState();
        }

        public async Task OnPostUpdateAreaNameAsync(int areaId, string areaName)
        {
            if (!string.IsNullOrWhiteSpace(areaName))
            {
                await _repoFactory.AreasRepository.UpdateArea(new DiaryArea { AreaId = areaId, AreaName = areaName });
            }
            else
            {
                ModelState.AddModelError("Theme Name", "Название темы не введено");
            }
            await UpdatePageState();
        }

        public async Task OnPostDeleteAreaAsync(int areaId)
        {
            if (await _repoFactory.AreasRepository.CanDeleteArea(areaId))
            {
                await _repoFactory.AreasRepository.DeleteArea(areaId);
            }
            else
            {
                ModelState.AddModelError("Area", "Область не может быть удалена. Есть связанные темы");
            }
            await UpdatePageState();
        }

        public async Task OnPostAddNewThemeAsync(int areaIdForTheme, string newRecordTypeName)
        {
            var area = await _repoFactory.AreasRepository.FetchAreaById(areaIdForTheme);
            if (area != null)
            {
                if (!string.IsNullOrWhiteSpace(newRecordTypeName))
                {
                    await _repoFactory.RecordTypesRepository.AddRecordType(areaIdForTheme, newRecordTypeName);
                }
                else
                {
                    ModelState.AddModelError("Theme Name", "Название новой темы не введено");
                }
            }
            else
            {
                ModelState.AddModelError("Theme Name", $"Область интересов с Id={areaIdForTheme} не найдена");
            }
            await UpdatePageState();
        }

        public async Task OnPostDeleteThemeAsync(int recordTypeId)
        {
            await _repoFactory.RecordTypesRepository.DeleteRecordType(recordTypeId);
            await UpdatePageState();
        }

        public async Task OnPostUpdateThemeNameAsync(int recordTypeId, string recordTypeName)
        {
            if (!string.IsNullOrWhiteSpace(recordTypeName))
            {
                await _repoFactory.RecordTypesRepository.UpdateRecordType(new DiaryRecordType { RecordTypeId = recordTypeId, RecordTypeName = recordTypeName });
            }
            else
            {
                ModelState.AddModelError("Theme Name", "Название темы не введено");
            }
            await UpdatePageState();
        }
    }
}