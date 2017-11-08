using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using RiseDiary.Domain.Model;
using System.Collections.Generic;

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

        private async Task UpdatePageState()
        {
            Records = await _repoFactory.RecordsRepository.FetchRecordsListFiltered(RecordsFilter.Empty);
        }

        public async Task OnGetAsync()
        {
            await UpdatePageState();
        }
    }
}