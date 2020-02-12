using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.WebUI.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Images
{
    internal class ImageListOption
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
    public class ImagesListFilteredModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public ImagesListFilteredModel(DiaryDbContext context)
        {
            _context = context;
        }

        public async Task<JsonResult> OnGetAsync(Guid recordId, string namePart)
        {
            var filteredImages = (await _context.FetchImagesFiltered(recordId, namePart))
                .Select(i => new ImageListOption { Id = i.Id, Name = i.Name })
                .ToList();

            return new JsonResult(filteredImages);
        }
    }
}