using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Domain.Repositories;
using Microsoft.Extensions.Logging;
using RiseDiary.Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RiseDiary.WebUI.Pages
{
    public class RecordEditModel : PageModel
    {
        private readonly IRepositoriesFactory _repoFactory;
        private readonly ILogger<ImagesPageModel> _logger;
        public RecordEditModel(IRepositoriesFactory factory, ILogger<ImagesPageModel> logger)
        {
            _repoFactory = factory;
            _logger = logger;
        }

        public int? RecordId { get; set; }
        public DateTime? RecordDate { get; set; }
        public DateTime? RecordCreateDate { get; set; }
        public DateTime? RecordModifyDate { get; set; }
        public string RecordName { get; set; }
        public string RecordText { get; set; }
        public IEnumerable<int> RecordThemesIds { get; set; }
        public IEnumerable<DiaryRecordTypeJoined> Themes { get; set; }
        public IEnumerable<DiaryImage> AddedImages { get; set; }
        public IEnumerable<DiaryImage> NotAddedImages { get; set; }

        private async Task UpdatePageState()
        {
            Themes = await _repoFactory.RecordTypesRepository.FetchRecordTypesWithAreas();
            RecordDate = DateTime.Now.Date;
            RecordThemesIds = new List<int>();
            if (RecordId != null)
            {
                var rec = await _repoFactory.RecordsRepository.FetchRecordById(RecordId.Value);
                if (rec != null)
                {
                    RecordDate = rec.RecordDate;
                    RecordCreateDate = rec.RecordCreateDate;
                    RecordModifyDate = rec.RecordModifyDate;
                    RecordName = rec.RecordName;
                    RecordText = rec.RecordText;
                    RecordThemesIds = (await _repoFactory.RecordTypesRepository.FetchTypesForRecord(rec.RecordId)).Select(rt => rt.RecordTypeId);

                    AddedImages = await _repoFactory.DiaryImagesRepository.FetchImagesForRecord(rec.RecordId);
                    NotAddedImages = (await _repoFactory.DiaryImagesRepository.FetchImageSet()).Except(AddedImages, new DiaryImageEqualityComparerById());
                }
            }
        }

        public async Task OnGetAsync(int? recordId)
        {
            RecordId = recordId;
            await UpdatePageState();
        }

        public async Task OnPostSaveRecordAsync(int? recordId, DateTime recordDate, string recordName, string recordText, int[] themeId)
        {
            if (recordId == null)
            {
                var newRecord = new DiaryRecord
                {
                    RecordCreateDate = DateTime.Now,
                    RecordModifyDate = DateTime.Now,
                    RecordDate = recordDate,
                    RecordName = recordName?.Trim() ?? string.Empty,
                    RecordText = recordText?.Trim() ?? string.Empty
                };
                int newRecordId = await _repoFactory.RecordsRepository.AddRecord(newRecord);
                foreach (int tid in themeId)
                {
                    await _repoFactory.RecordTypesRepository.AddTypeForRecord(newRecordId, tid);
                }
                RecordId = newRecordId;
            }
            else
            {
                var record = await _repoFactory.RecordsRepository.FetchRecordById(recordId.Value);
                if (record != null)
                {
                    record.RecordDate = recordDate;
                    record.RecordModifyDate = DateTime.Now;
                    record.RecordName = recordName?.Trim() ?? string.Empty;
                    record.RecordText = recordText?.Trim() ?? string.Empty;
                    await _repoFactory.RecordsRepository.UpdateRecord(record);

                    var recThemesIds = (await _repoFactory.RecordTypesRepository.FetchTypesForRecord(record.RecordId)).Select(rth => rth.RecordTypeId);
                    foreach (int id in recThemesIds.Except(themeId))
                    {
                        await _repoFactory.RecordTypesRepository.RemoveTypeForRecord(record.RecordId, id);
                    }
                    foreach (int id in themeId.Except(recThemesIds))
                    {
                        await _repoFactory.RecordTypesRepository.AddTypeForRecord(record.RecordId, id);
                    }
                    RecordId = record.RecordId;
                }
            }
            await UpdatePageState();
        }

        public async Task<IActionResult> OnPostDeleteRecordAsync(int recordId)
        {
            await _repoFactory.RecordsRepository.DeleteRecord(recordId);
            return RedirectToPage("RecordsView");
        }

        public async Task OnPostAddImageAsync(int recordId, int imageId)
        {           
            if(recordId != 0) RecordId = recordId;
            if (imageId != 0)
            {                
                await _repoFactory.DiaryImagesRepository.AddImageForRecord(recordId, imageId);
            }
            await UpdatePageState();
        }

        public async Task OnPostDeleteImageAsync(int recordId, int imageId)
        {
            if (recordId != 0) RecordId = recordId;
            if (imageId != 0)
            {
                await _repoFactory.DiaryImagesRepository.RemoveImageForRecord(recordId, imageId);
            }
            await UpdatePageState();
        }
    }
}