using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.WebUI.Data;
using System;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Images
{
    public class ImageFileModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public ImageFileModel(DiaryDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> OnGetAsync(Guid imageId)
        {
            if(imageId == Guid.Empty) return NotFound();

            var tmpImg = await _context.FetchTempImage(imageId);
            if (tmpImg != null)
            {
                return File(tmpImg.Data, "image/jpeg");
            }
            else
            {
                var image = await _context.FetchFullImageById(imageId);
                if (image == null) return NotFound();
                return File(image, "image/jpeg");
            }           
        }
    }
}