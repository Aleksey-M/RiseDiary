using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Images
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
    public class EditModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public EditModel(DiaryDbContext context)
        {
            _context = context;
        }

        public Guid? RecordId { get; private set; }
        public Guid ImageId { get; private set; }
        public DiaryImage Image { get; private set; } = null!;
        public int BiggestImageDimm => Image.Width > Image.Height ? Image.Width : Image.Height;
        public TempImage? TempImage { get; private set; }
        public Uri ImageUrl { get; private set; } = null!;
        public Dictionary<Guid, string> ImageLinks { get; private set; } = new Dictionary<Guid, string>();

        private async Task UpdateModel()
        {
            string url = Request.Scheme + @"://" + Request.Host.Host + ":" + Request.Host.Port;

            Image = await _context.FetchImageById(ImageId);
            ImageUrl = new Uri($@"{url}/Images/ImageFile/{Image.Id.ToString()}");
            TempImage = await _context.FetchTempImage(ImageId);
            ImageLinks = await _context.FetchRecordsForImage(ImageId) ?? new Dictionary<Guid, string>();
        }

        public async Task<IActionResult> OnGetAsync(Guid? imageId, Guid? recordId)
        {
            if (imageId == null || imageId.Value == Guid.Empty) return Redirect("/images/index");

            RecordId = recordId;
            ImageId = imageId.Value;
            await UpdateModel();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteImageAsync(Guid? imageId, Guid? recordId)
        {
            if (imageId != null && imageId.Value != Guid.Empty)
            {
                await _context.DeleteTempImage(imageId.Value);
                await _context.DeleteImage(imageId.Value);
            }
            return (recordId != null && recordId != Guid.Empty) ? Redirect($"/Records/View?recordid={recordId.Value.ToString()}") : Redirect("/images/index");
        }

        public async Task OnPostReplaceImageAsync(IFormFile newImage, Guid imageId, Guid? recordId)
        {
            if (imageId != Guid.Empty)
            {
                RecordId = recordId;
                ImageId = imageId;
                if (newImage != null)
                {
                    byte[]? imageData = null;
                    using (var binaryReader = new BinaryReader(newImage.OpenReadStream()))
                    {
                        imageData = binaryReader.ReadBytes((int)newImage.Length);
                    }
                    var image = await _context.FetchImageById(ImageId);
                    var uploaded = ImageHelper.ReplaceImage(image, imageData);
                    await _context.AddUnsavedTempImage(uploaded);
                }
                await UpdateModel();
            }
        }

        public async Task OnPostCancelEditAsync(Guid imageId, Guid? recordId)
        {
            RecordId = recordId;
            ImageId = imageId;
            await _context.DeleteTempImage(ImageId);
            await UpdateModel();
        }

        public async Task OnPostSaveUpdatedImageAsync(Guid imageId, Guid? recordId)
        {
            RecordId = recordId;
            ImageId = imageId;
            var tmpImage = await _context.FetchTempImage(ImageId);
            int imageQuality = await _context.GetAppSettingInt(AppSettingsKeys.ImageQuality) ?? 75;
            await _context.ApplyChangesFromTempImage(tmpImage, imageQuality);
            await UpdateModel();
        }

        public async Task OnPostSaveUpdatedAsNewImageAsync(Guid imageId, Guid? recordId)
        {
            var tmpImage = await _context.FetchTempImage(imageId);
            var image = await _context.FetchImageById(imageId);
            int imageQuality = await _context.GetAppSettingInt(AppSettingsKeys.ImageQuality) ?? 75;
            ImageId = await _context.AddImage($"{image.Name} ({tmpImage.Modification})", tmpImage.Data, imageQuality, taken: image.Taken);
            await _context.DeleteTempImage(imageId);
            RecordId = recordId;
            if (RecordId != null && RecordId.Value != Guid.Empty)
            {
                await _context.AddRecordImage(RecordId.Value, ImageId);
            }
            await UpdateModel();
        }

        public async Task OnPostScaleImageAsync(Guid imageId, int imageSize, Guid? recordId)
        {
            RecordId = recordId;
            ImageId = imageId;
            Image = await _context.FetchImageById(ImageId);
            if (imageSize > 0 && imageSize <= BiggestImageDimm)
            {
                var image = await _context.FetchImageById(imageId);
                var sourceImage = await _context.FetchFullImageById(imageId);
                int imageQuality = await _context.GetAppSettingInt(AppSettingsKeys.ImageQuality) ?? 75;
                var tmpImage = ImageHelper.ScaleImage(image, sourceImage, imageSize, imageQuality);
                await _context.AddUnsavedTempImage(tmpImage);
            }
            await UpdateModel();
        }

    }
}