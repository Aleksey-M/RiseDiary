using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Images
{
    public class IndexModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public List<DiaryImage> Images { get; private set; }
        private const int _pageSize = 32;
        public int CurrenPage { get; private set; }
        public int NextPage { get; private set; }
        public int PrevPage { get; private set; }
        public int PagesCount { get; private set; }
        public int ImagesCount { get; private set; }
        public IndexModel(DiaryDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync(int pageNo)
        {
            ImagesCount = await _context.GetImagesCount();
            PagesCount = Convert.ToInt32(Math.Ceiling(ImagesCount / (double)_pageSize));
            CurrenPage = pageNo <= 0 ? 1 : pageNo;
            CurrenPage = CurrenPage > PagesCount ? PagesCount : CurrenPage;
            NextPage = CurrenPage == PagesCount ? CurrenPage : CurrenPage + 1;
            PrevPage = CurrenPage == 1 ? 1 : CurrenPage - 1;
            Images = await _context.FetchImageSet((CurrenPage - 1) * _pageSize, _pageSize);
        }
    }
}