using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Pages.Images
{
    public class ViewModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public ViewModel(DiaryDbContext context)
        {
            _context = context;
        }

        public int ImageId { get; private set; }
        public DiaryImage Image { get; private set; }
        public byte[] FullImage { get; private set; }
        public string FullImageString => Convert.ToBase64String(FullImage);
        public Dictionary<int, string> ImageLinks { get; private set; }
        public string ImageUrl { get; private set; }

        private async Task UpdateModel()
        {
            ImageUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/Images/ImageFile/{ImageId}";
            Image = await _context.FetchImageById(ImageId);
            FullImage = await _context.FetchFullImageById(ImageId);
            ImageLinks = await _context.FetchRecordsForImage(ImageId);
        }

        public async Task<IActionResult> OnGetAsync(int? imageId)
        {
            if(imageId == null)
            {
                return Redirect("/Images/Index");
            }
            ImageId = imageId.Value;
            await UpdateModel();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateImageNameAsync(int imageId, string imageName)
        {
            if (imageId == 0)
            {
                return Redirect("/Images/Index");
            }
            ImageId = imageId;
            if (string.IsNullOrWhiteSpace(imageName))
            {
                ModelState.AddModelError("Image", "Не введено новое название картинки");
            }
            else
            {
                await _context.UpdateImageName(imageId, imageName);
            }
            await UpdateModel();
            return Page();
        }
        
    }
}