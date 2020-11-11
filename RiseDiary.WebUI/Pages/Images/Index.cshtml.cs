using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Images
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
    public class IndexModel : PageModel
    {
        private readonly IImagesService _imagesService;
        public IEnumerable<DiaryImage> Images { get; private set; } = Enumerable.Empty<DiaryImage>();
        private const int _pageSize = 32;
        public int CurrenPage { get; private set; }
        public int NextPage { get; private set; }
        public int PrevPage { get; private set; }
        public int PagesCount { get; private set; }
        public int ImagesCount { get; private set; }
        public IndexModel(IImagesService imagesService)
        {
            _imagesService = imagesService;
        }

        public async Task OnGetAsync(int pageNo)
        {
            ImagesCount = await _imagesService.GetImagesCount();
            PagesCount = Convert.ToInt32(Math.Ceiling(ImagesCount / (double)_pageSize));

            CurrenPage = pageNo;
            if (CurrenPage > PagesCount || CurrenPage < 0) CurrenPage = 0;

            NextPage = CurrenPage >= PagesCount - 1 ? CurrenPage : CurrenPage + 1;
            PrevPage = CurrenPage == 0 ? 0 : CurrenPage - 1;
            Images = await _imagesService.FetchImageSet(CurrenPage * _pageSize, _pageSize);
        }
    }
}