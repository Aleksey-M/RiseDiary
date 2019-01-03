using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Pages.Images
{
    public class EditModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public EditModel(DiaryDbContext context)
        {
            _context = context;
        }

        public int RecordId { get; private set; }
        public int ImageId { get; private set; }
        public DiaryImage Image { get; private set; }
        public byte[] FullImage { get; private set; }
        public string FullImageString => Convert.ToBase64String(FullImage);
        public string FullModifiedImageString => Convert.ToBase64String(TempImage?.Data);
        public int BiggestImageDimm => Image.Width > Image.Height ? Image.Width : Image.Height;
        public TempImage TempImage { get; private set; }
        public string ImageUrl { get; private set; }
        public Dictionary<int, string> ImageLinks { get; private set; }

        private async Task UpdateModel()
        {
            ImageUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/Images/ImageFile/{ImageId}";
            Image = await _context.FetchImageById(ImageId);
            FullImage = await _context.FetchFullImageById(ImageId);
            TempImage = await _context.FetchTempImage(ImageId);
            ImageLinks = await _context.FetchRecordsForImage(ImageId);
        }

        public async Task<IActionResult> OnGetAsync(int imageId, int recordId)
        {
            if(imageId == 0)
            {
                return Redirect("/images/index");
            }
            ImageId = imageId;
            RecordId = recordId;

            await UpdateModel();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteImageAsync(int? imageId, int recordId)
        {
            if(imageId != null && imageId.Value != 0)
            {
                await _context.DeleteTempImage(imageId.Value);
                await _context.DeleteImage(imageId.Value);
            }
            return recordId != 0 ? Redirect($"/Records/View?recordid={recordId}") : Redirect("/images/index");
        }

        public async Task OnPostReplaceImageAsync(IFormFile newImage, int imageId, int recordId)
        {
            RecordId = recordId;
            ImageId = imageId;
            if (newImage != null && ImageId != 0)
            {
                byte[] imageData = null;
                using (var binaryReader = new BinaryReader(newImage.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)newImage.Length);
                }
                var image = await _context.FetchImageById(imageId);
                var uploaded = ImageHelper.ReplaceImage(image, imageData);
                await _context.AddUnsavedTempImage(uploaded);
            }
            await UpdateModel();
        }

        public async Task OnPostCancelEditAsync(int imageId, int recordId)
        {
            RecordId = recordId;
            ImageId = imageId;
            await _context.DeleteTempImage(ImageId);
            await UpdateModel();
        }
        public async Task OnPostSaveUpdatedImageAsync(int imageId, int recordId)
        {
            RecordId = recordId;
            ImageId = imageId;
            var tmpImage = await _context.FetchTempImage(ImageId);
            await _context.ApplyChangesFromTempImage(tmpImage);
            await UpdateModel();
        }

        public async Task OnPostSaveUpdatedAsNewImageAsync(int imageId, int recordId)
        {            
            var tmpImage = await _context.FetchTempImage(imageId);
            var image = await _context.FetchImageById(imageId);
            ImageId = await _context.AddImage($"{image.Name} ({tmpImage.Modification})", tmpImage.Data);
            await _context.DeleteTempImage(imageId);
            RecordId = recordId;
            if(RecordId != 0)
            {
                await _context.AddRecordImage(RecordId, ImageId);
            }
            await UpdateModel();
        }
        
        public async Task OnPostScaleImageAsync(int imageId, int imageSize, int recordId)
        {
            RecordId = recordId;
            ImageId = imageId;
            if(imageSize > 0)
            {
                var image = await _context.FetchImageById(imageId);
                var sourceImage = await _context.FetchFullImageById(imageId);
                var tmpImage = ImageHelper.ScaleImage(image, sourceImage, imageSize);
                await _context.AddUnsavedTempImage(tmpImage);
            }
            await UpdateModel();
        }

        public async Task<IActionResult> OnPostUpdateImageNameAsync(int imageId, string imageName)
        {
            if (imageId == 0)
            {
                return Redirect("/Images/Index");
            }
            ImageId = imageId;
            if (!string.IsNullOrWhiteSpace(imageName))
            {
                await _context.UpdateImageName(imageId, imageName);
            }
            await UpdateModel();
            return Page();
        }
    }
}