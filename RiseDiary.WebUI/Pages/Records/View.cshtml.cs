using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages
{
    public class RecordViewModel : PageModel
    {
        private readonly IRecordsService _recordsService;
        private readonly IRecordsImagesService _recordsImagesService;

        public RecordViewModel(IRecordsService recordsService, IRecordsImagesService recordsImagesService)
        {
            _recordsService = recordsService;
            _recordsImagesService = recordsImagesService;
        }

        public Guid RecordId { get; set; }
        public DiaryRecord Record { get; set; } = null!;
        public IEnumerable<string> RecordThemes { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<(Guid ImageId, string ImageName)> RecordImages { get; private set; } = Enumerable.Empty<(Guid ImageId, string ImageName)>();
        public IEnumerable<Cogitation> Cogitations { get; private set; } = Enumerable.Empty<Cogitation>();
        private async Task UpdatePageState()
        {
            Record = await _recordsService.FetchRecordById(RecordId) ?? throw new ArgumentException($"Record with Id = '{RecordId}' is not exists");
            RecordThemes = Record.ThemesRefs.Select(tr => tr.Theme?.ThemeName ?? "").ToList();
            RecordImages = Record.ImagesRefs.Select(ir => (ir.ImageId, ir.Image?.Name ?? "")).ToList();
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
                await _recordsService.AddCogitation(recordId, newCogText);
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
                await _recordsService.DeleteCogitation(cogitationId);
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
                await _recordsService.UpdateCogitationText(cogitationId, cogText);
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
                await _recordsImagesService.RemoveRecordImage(recordId, imageId);
            }
            await UpdatePageState();
        }

        public async Task OnPostAddImageAsync(Guid recordId, Guid imageId)
        {
            if (recordId != Guid.Empty) RecordId = recordId;
            if (imageId != Guid.Empty)
            {
                await _recordsImagesService.AddRecordImage(recordId, imageId);
            }
            await UpdatePageState();
        }
    }
}