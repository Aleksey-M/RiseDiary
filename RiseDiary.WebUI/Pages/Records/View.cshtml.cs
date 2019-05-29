using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System;
using RiseDiary.WebUI.Data;
using RiseDiary.Model;

namespace RiseDiary.WebUI.Pages
{
    public class RecordViewModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public RecordViewModel(DiaryDbContext context)
        {
            _context = context;
        }

        public int RecordId { get; set; }
        public DiaryRecord  Record { get; set; }
        public List<string> RecordThemes { get; set; }
        public List<(int ImageId, string ImageName)> RecordImages { get; set; }
        public List<Cogitation> Cogitations { get; set; }

        private async Task UpdatePageState()
        {
            Record = await _context.FetchRecordByIdWithData(RecordId);
            RecordThemes = Record.ThemesRefs.Select(tr => tr.Theme.ThemeName).ToList();
            RecordImages = Record.ImagesRefs.Select(ir => (ir.ImageId, ir.Image.Name)).ToList();
            Cogitations = Record.Cogitations.OrderBy(c => c.Date).ToList();
        }

        public async Task<IActionResult> OnGetAsync(string recordId)
        {
            if(int.TryParse(recordId, out int id))
            {
                RecordId = id;
            }
            else
            {
                var idByCode = await _context.FetchRecordIdByCode(recordId);
                if(idByCode == null) return Redirect("/Records/Edit");
                RecordId = idByCode.Value;
            }

            await UpdatePageState();
            return null;
        }

        public async Task OnPostAddCogitationAsync(int recordId, string newCogText)
        {
            if (!string.IsNullOrWhiteSpace(newCogText) && recordId != 0)
            {
                await _context.AddCogitation(new Cogitation
                {
                    Date = DateTime.Now,
                    RecordId = recordId,
                    Text = newCogText
                });                
            }       
            if(recordId != 0)
            {
                RecordId = recordId;
                await UpdatePageState();
            }
        }

        public async Task OnPostDeleteCogitationAsync(int recordId, int cogitationId)
        {
            if(cogitationId != 0)
            {
                await _context.DeleteCogitation(cogitationId);
            }
            if(recordId != 0)
            {
                RecordId = recordId;
                await UpdatePageState();
            }
        }

        public async Task OnPostSaveCogitationAsync(int recordId, int cogitationId, string cogText)
        {
            if(cogitationId != 0 && !string.IsNullOrWhiteSpace(cogText))
            {
                await _context.UpdateCogitationText(cogitationId, cogText);
            }
            if (recordId != 0)
            {
                RecordId = recordId;
                await UpdatePageState();
            }
        }

        public async Task OnPostDeleteImageAsync(int recordId, int imageId)
        {
            if (recordId != 0) RecordId = recordId;
            if (imageId != 0)
            {
                await _context.RemoveRecordImage(recordId, imageId);
            }            
            await UpdatePageState();
        }

        public async Task OnPostAddImageAsync(int recordId, int imageId)
        {
            if (recordId != 0) RecordId = recordId;
            if (imageId != 0)
            {
                await _context.AddRecordImage(recordId, imageId);
            }
            await UpdatePageState();
        }
    }
}