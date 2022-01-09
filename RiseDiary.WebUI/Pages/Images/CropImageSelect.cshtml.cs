using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Images
{
    public class CropImageSelectModel : PageModel
    {
        private readonly ICropImageService _cropImageService;

        public CropImageSelectModel(ICropImageService cropImageService)
        {
            _cropImageService = cropImageService ?? throw new ArgumentNullException(nameof(cropImageService));
        }

        public Guid ImageId { get; private set; }

        public Guid RecordId { get; private set; }

        public ScaledImagePreview ScaledImage { get; private set; } = null!;

        public string ScaledImageString => Convert.ToBase64String(ScaledImage.Image);


        public async Task<ActionResult> OnGetAsync(Guid? imageId, Guid? recordId)
        {
            ImageId = imageId ?? Guid.Empty;
            RecordId = recordId ?? Guid.Empty;
            if (imageId == Guid.Empty)
            {
                return Redirect("Images");
            }

            ScaledImage = await _cropImageService.CreateScaledImagePreview(ImageId);

            return Page();
        }

        public async Task<RedirectToPageResult> OnPostCropImageAsync(Guid? imageId, int selLeft, int selTop, int selWidth, int selHeight, double coefficient, Guid? recordId)
        {
            RecordId = recordId ?? Guid.Empty;
            ImageId = imageId ?? Guid.Empty;

            if (selLeft >= 0 && selTop >= 0 && selWidth > 0 && selHeight > 0 && coefficient >= 1)
            {
                var rect = new Rectangle(selLeft, selTop, selWidth, selHeight);
                await _cropImageService.CropImage(ImageId, rect, coefficient);
            }

            return RedirectToPage("Edit", new { ImageId, RecordId });
        }
    }
}