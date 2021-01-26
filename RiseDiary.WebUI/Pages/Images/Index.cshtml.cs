using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Images
{
    public class IndexModel : PageModel
    {
        private readonly IImagesService _imagesService;

        public IndexModel(IImagesService imagesService)
        {
            _imagesService = imagesService;
        }

        public IEnumerable<DiaryImage> Images { get; private set; } = Enumerable.Empty<DiaryImage>();

        private const int _pageSize = 32;

        public PagesInfo? Pager { get; private set; }

        public async Task OnGetAsync(int? pageNo)
        {
            var imagesCount = await _imagesService.GetImagesCount();
            Pager = PagesInfo.GetPagesInfo(imagesCount, pageNo ?? 1, _pageSize);
            Images = await _imagesService.FetchImageSet(Pager.StartIndex, Pager.PageSize);
        }
    }
}