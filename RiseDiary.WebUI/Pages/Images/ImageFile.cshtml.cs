using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.WebUI.Data;
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
        public async Task<IActionResult> OnGetAsync(string imageId)
        {
            if(int.TryParse(imageId, out int id))
            {
                if (id == 0) return BadRequest();
                var image = await _context.FetchFullImageById(id);
                if (image == null) return NotFound();
                return File(image, "image/jpeg");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(imageId)) return BadRequest();
                var image = await _context.FetchFullImageByCode(imageId);
                if (image == null) return NotFound();
                return File(image, "image/jpeg");
            }            
        }
    }
}