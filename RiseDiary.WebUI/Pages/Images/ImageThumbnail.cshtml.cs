using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Pages.Images
{
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
            //if (Guid.TryParse(imageId, out int id))
            //{
            //    if (id == 0) return BadRequest();
            //    var image = await _context.FetchImageById(id);
            //    if (image == null) return NotFound();
            //    return File(image.Thumbnail, "image/jpeg");
            //}
            //else
            //{
            //    if (string.IsNullOrWhiteSpace(imageId)) return BadRequest();
            //    var image = await _context.FetchImageByCode(imageId);
            //    if (image == null) return NotFound();
            //    return File(image.Thumbnail, "image/jpeg");
            //}
        }
    }
}