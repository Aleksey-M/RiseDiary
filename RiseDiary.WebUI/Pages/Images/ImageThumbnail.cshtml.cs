using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.WebUI.Data;
using System;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Images
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
    public class ImageThumbnailModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public ImageThumbnailModel(DiaryDbContext context)
        {
            _context = context;
        }
        public async Task<ActionResult> OnGetAsync(Guid imageId)
        {
            if (imageId == Guid.Empty) return BadRequest();
            var image = await _context.FetchImageById(imageId);
            if (image == null) return NotFound();
            return File(image.Thumbnail, "image/jpeg");
        }
    }
}