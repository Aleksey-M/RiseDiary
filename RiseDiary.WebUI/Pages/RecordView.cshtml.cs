using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Domain.Repositories;
using Microsoft.Extensions.Logging;
using RiseDiary.Domain.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RiseDiary.WebUI.Pages
{
    public class RecordViewModel : PageModel
    {
        private readonly IRepositoriesFactory _repoFactory;
        private readonly ILogger<RecordViewModel> _logger;
        public RecordViewModel(IRepositoriesFactory factory, ILogger<RecordViewModel> logger)
        {
            _repoFactory = factory;
            _logger = logger;
        }

        public int RecordId { get; set; }
        public DiaryRecord  Record { get; set; }
        public List<string> RecordThemes { get; set; }
        public Dictionary<string, string> RecordImages { get; set; }
        public List<Cogitation> Cogitations { get; set; }

        private async Task UpdatePageState()
        {
            Record = await _repoFactory.RecordsRepository.FetchRecordById(RecordId);
            var themes = await _repoFactory.RecordTypesRepository.FetchTypesForRecord(Record.RecordId);
            RecordThemes = themes.Select(t => t.RecordTypeName).ToList();
            RecordImages = new Dictionary<string, string>();
            byte[] imgBytes = null;
            foreach (var img in await _repoFactory.DiaryImagesRepository.FetchImagesForRecord(Record.RecordId))
            {
                imgBytes = await _repoFactory.DiaryImagesRepository.FetchImageDataById(img.ImageId);
                RecordImages.Add(img.ImageName, Convert.ToBase64String(imgBytes));
            }
            Cogitations = (await _repoFactory.CogitationRepository.FetchAllCogitationsOfRecord(Record.RecordId)).OrderBy(c => c.CogitationDate).ToList();
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
                await _repoFactory.CogitationRepository.AddCogitation(new Cogitation
                {
                    CogitationDate = DateTime.Now,
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
                await _repoFactory.CogitationRepository.DeleteCogitation(cogitationId);
            }
            if(recordId != 0)
            {
                RecordId = recordId;
                await UpdatePageState();
            }
        }
    }
}