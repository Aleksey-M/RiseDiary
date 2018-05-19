﻿using System.Threading.Tasks;
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
        private readonly ILogger<RecordViewModel> _logger;
        public RecordViewModel(DiaryDbContext context, ILogger<RecordViewModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public int RecordId { get; set; }
        public DiaryRecord  Record { get; set; }
        public List<string> RecordThemes { get; set; }
        public Dictionary<string, string> RecordImages { get; set; }
        public List<Cogitation> Cogitations { get; set; }

        private async Task UpdatePageState()
        {
            Record = await _context.FetchRecordById(RecordId);
            var themes = await _context.FetchRecordThemes(Record.Id);
            RecordThemes = themes.Select(t => t.ThemeName).ToList();
            RecordImages = new Dictionary<string, string>();
            byte[] imgBytes = null;
            foreach (var img in await _context.FetchImagesForRecord(Record.Id))
            {
                imgBytes = await _context.FetchImageDataById(img.Id);
                RecordImages.Add(img.Name, Convert.ToBase64String(imgBytes));
            }
            Cogitations = (await _context.FetchAllCogitationsOfRecord(Record.Id)).OrderBy(c => c.Date).ToList();
        }

        public async Task<IActionResult> OnGetAsync(int? recordId)
        {
            if (recordId == null || recordId.Value == 0)
                return Redirect("/RecordEdit");
            else
            {
                RecordId = recordId.Value;
                await UpdatePageState();
                return null;
            }
        }

        public async Task OnPostAddCogitationAsync(int recordId, string cogText)
        {
            if (!string.IsNullOrWhiteSpace(cogText) && recordId != 0)
            {
                await _context.AddCogitation(new Cogitation
                {
                    Date = DateTime.Now,
                    RecordId = recordId,
                    Text = cogText
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
    }
}