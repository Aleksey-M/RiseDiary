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
        private readonly IAppSettingsService _appSettingsService;

        public IndexModel(IImagesService imagesService, IAppSettingsService appSettingsService)
        {
            _imagesService = imagesService;
            _appSettingsService = appSettingsService;
        }

        public IEnumerable<DiaryImage> Images { get; private set; } = Enumerable.Empty<DiaryImage>();

        public PagesInfo? Pager { get; private set; }

        public async Task OnGetAsync(int? pageNo)
        {
            var imagesCount = await _imagesService.GetImagesCount();
            int pageSize = await _appSettingsService.GetAppSettingInt(AppSettingsKey.ImagesPageSize) ?? 32;
            Pager = PagesInfo.GetPagesInfo(imagesCount, pageNo ?? 1, pageSize);
            Images = await _imagesService.FetchImageSet(Pager.StartIndex, Pager.PageSize);
        }
    }
}