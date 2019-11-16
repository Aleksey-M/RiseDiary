﻿using Microsoft.AspNetCore.Http;
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
    public class EditModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public EditModel(DiaryDbContext context)
        {
            _context = context;
        }

        public Guid? RecordId { get; private set; }
        public Guid ImageId { get; private set; }
        public DiaryImage Image { get; private set; }
        public int BiggestImageDimm => Image.Width > Image.Height ? Image.Width : Image.Height;
        public TempImage TempImage { get; private set; }
        public string ImageUrl { get; private set; }
        public Dictionary<Guid, string> ImageLinks { get; private set; }

        private async Task UpdateModel()
        {
            string url = Request.Scheme + @"://" + Request.Host.Host + ":" + Request.Host.Port;

            Image = await _context.FetchImageById(ImageId);
            ImageUrl = $@"{url}/Images/ImageFile/{Image.Id.ToString()}";
            TempImage = await _context.FetchTempImage(ImageId);
            ImageLinks = await _context.FetchRecordsForImage(ImageId);
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

        public async Task OnPostReplaceImageAsync(IFormFile newImage, Guid? imageId, Guid? recordId)
        {
            if (imageId != null || imageId.Value != Guid.Empty)
            {
                RecordId = recordId;
                ImageId = imageId.Value;
                if (newImage != null)
                {
                    byte[] imageData = null;
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
            await _context.ApplyChangesFromTempImage(tmpImage);
            await UpdateModel();
        }

        public async Task OnPostSaveUpdatedAsNewImageAsync(Guid imageId, Guid? recordId)
        {
            var tmpImage = await _context.FetchTempImage(imageId);
            var image = await _context.FetchImageById(imageId);
            ImageId = await _context.AddImage($"{image.Name} ({tmpImage.Modification})", tmpImage.Data);
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
            if (imageSize > 0)
            {
                var image = await _context.FetchImageById(imageId);
                var sourceImage = await _context.FetchFullImageById(imageId);
                var tmpImage = ImageHelper.ScaleImage(image, sourceImage, imageSize);
                await _context.AddUnsavedTempImage(tmpImage);
            }
            await UpdateModel();
        }

        public async Task<IActionResult> OnPostUpdateImageNameAsync(Guid imageId, string imageName)
        {
            if (imageId == Guid.Empty)
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