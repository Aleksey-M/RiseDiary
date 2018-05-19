using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.WebUI.Data;
using RiseDiary.Model;

namespace RiseDiary.WebUI.Pages
{
    public class RecordEditModel : PageModel
    {
        private readonly DiaryDbContext _context;
        private readonly ILogger<RecordEditModel> _logger;
        public RecordEditModel(DiaryDbContext context, ILogger<RecordEditModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public int? RecordId { get; set; }
        public DateTime? RecordDate { get; set; }
        public DateTime? RecordCreateDate { get; set; }
        public DateTime? RecordModifyDate { get; set; }
        public string RecordName { get; set; }
        public string RecordText { get; set; }
        public IEnumerable<int> RecordThemesIds { get; set; }
        public IEnumerable<DiaryThemeJoined> Themes { get; set; }
        public IEnumerable<DiaryImage> AddedImages { get; set; }
        public IEnumerable<DiaryImage> NotAddedImages { get; set; }

        private async Task UpdatePageState()
        {
            Themes = await _context.FetchThemesWithScopes();
            RecordDate = DateTime.Now.Date;
            RecordThemesIds = new List<int>();
            if (RecordId != null)
            {
                var rec = await _context.FetchRecordById(RecordId.Value);
                if (rec != null)
                {
                    RecordDate = rec.Date;
                    RecordCreateDate = rec.CreateDate;
                    RecordModifyDate = rec.ModifyDate;
                    RecordName = rec.Name;
                    RecordText = rec.Text;
                    RecordThemesIds = (await _context.FetchRecordThemes(rec.Id)).Select(rt => rt.Id);

                    AddedImages = await _context.FetchImagesForRecord(rec.Id);
                    int imagesCount = await _context.GetImagesCount();
                    NotAddedImages = (await _context.FetchImageSet(0, imagesCount)).Except(AddedImages, new DiaryImageEqualityComparerById());
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
                    CreateDate = DateTime.Now,
                    ModifyDate = DateTime.Now,
                    Date = recordDate,
                    Name = recordName?.Trim() ?? string.Empty,
                    Text = recordText?.Trim() ?? string.Empty
                };
                int newRecordId = await _context.AddDiaryRecord(newRecord);
                foreach (int tid in themeId)
                {
                    await _context.AddRecordTheme(newRecordId, tid);
                }
                RecordId = newRecordId;
            }
            else
            {
                var record = await _context.FetchRecordById(recordId.Value);
                if (record != null)
                {
                    record.Date = recordDate;
                    record.ModifyDate = DateTime.Now;
                    record.Name = recordName?.Trim() ?? string.Empty;
                    record.Text = recordText?.Trim() ?? string.Empty;
                    await _context.UpdateRecord(record);

                    var recThemesIds = (await _context.FetchRecordThemes(record.Id)).Select(rth => rth.Id);
                    foreach (int id in recThemesIds.Except(themeId))
                    {
                        await _context.RemoveRecordTheme(record.Id, id);
                    }
                    foreach (int id in themeId.Except(recThemesIds))
                    {
                        await _context.AddRecordTheme(record.Id, id);
                    }
                    RecordId = record.Id;
                }
            }
            await UpdatePageState();
        }

        public async Task<IActionResult> OnPostDeleteRecordAsync(int recordId)
        {
            await _context.DeleteRecord(recordId);
            return RedirectToPage("RecordsView");
        }

        public async Task OnPostAddImageAsync(int recordId, int imageId)
        {           
            if(recordId != 0) RecordId = recordId;
            if (imageId != 0)
            {                
                await _context.AddImageForRecord(recordId, imageId);
            }
            await UpdatePageState();
        }

        public async Task OnPostDeleteImageAsync(int recordId, int imageId)
        {
            if (recordId != 0) RecordId = recordId;
            if (imageId != 0)
            {
                await _context.RemoveImageForRecord(recordId, imageId);
            }
            await UpdatePageState();
        }
    }
}