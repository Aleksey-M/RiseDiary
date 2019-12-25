using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
    public class RecordViewModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public RecordViewModel(DiaryDbContext context)
        {
            _context = context;
        }

        public Guid RecordId { get; set; }
        public DiaryRecord Record { get; set; } = null!;
        public IEnumerable<string> RecordThemes { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<(Guid ImageId, string ImageName)> RecordImages { get; private set; } = Enumerable.Empty<(Guid ImageId, string ImageName)>();
        public IEnumerable<Cogitation> Cogitations { get; private set; } = Enumerable.Empty<Cogitation>();
        private string LocalHostAndPort => Request.Scheme + @"://" + Request.Host.Host + ":" + Request.Host.Port;
        private async Task UpdatePageState()
        {
            Record = await _context.FetchRecordByIdWithData(RecordId, LocalHostAndPort) ?? throw new ArgumentException($"Record with Id = '{RecordId}' is not exists");
            RecordThemes = Record.ThemesRefs.Select(tr => tr.Theme?.ThemeName ?? string.Empty).ToList();
            RecordImages = Record.ImagesRefs.Select(ir => (ir.ImageId, ir.Image.Name)).ToList();
            Cogitations = Record.Cogitations.OrderBy(c => c.Date).ToList();
        }

        public async Task<IActionResult> OnGetAsync(Guid recordId)
        {
            RecordId = recordId;
            await UpdatePageState();
            return Page();
        }

        public async Task OnPostAddCogitationAsync(Guid recordId, string newCogText)
        {
            if (!string.IsNullOrWhiteSpace(newCogText) && recordId != Guid.Empty)
            {
                await _context.AddCogitation(new Cogitation
                {
                    Date = DateTime.Now,
                    RecordId = recordId,
                    Text = newCogText
                },
                LocalHostAndPort);
            }
            if (recordId != Guid.Empty)
            {
                RecordId = recordId;
                await UpdatePageState();
            }
        }

        public async Task OnPostDeleteCogitationAsync(Guid recordId, Guid cogitationId)
        {
            if (cogitationId != Guid.Empty)
            {
                await _context.DeleteCogitation(cogitationId);
            }
            if (recordId != Guid.Empty)
            {
                RecordId = recordId;
                await UpdatePageState();
            }
        }
                
        public async Task OnPostSaveCogitationAsync(Guid recordId, Guid cogitationId, string cogText)
        {
            if (cogitationId != Guid.Empty && !string.IsNullOrWhiteSpace(cogText))
            {
                await _context.UpdateCogitationText(cogitationId, cogText, LocalHostAndPort);
            }
            if (recordId != Guid.Empty)
            {
                RecordId = recordId;
                await UpdatePageState();
            }
        }

        public async Task OnPostDeleteImageAsync(Guid recordId, Guid imageId)
        {
            if (recordId != Guid.Empty) RecordId = recordId;
            if (imageId != Guid.Empty)
            {
                await _context.RemoveRecordImage(recordId, imageId);
            }
            await UpdatePageState();
        }

        public async Task OnPostAddImageAsync(Guid recordId, Guid imageId)
        {
            if (recordId != Guid.Empty) RecordId = recordId;
            if (imageId != Guid.Empty)
            {
                await _context.AddRecordImage(recordId, imageId);
            }
            await UpdatePageState();
        }
    }
}