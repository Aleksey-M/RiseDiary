﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Pages.Images
{
    public class CropImageSelectModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public CropImageSelectModel(DiaryDbContext context)
        {
            _context = context;
        }
        private const int MaxScaledWidth = 750;
        private const int MaxScaledHeight = 600;
        public int ImageId { get; private set; }
        public int RecordId { get; private set; }
        public byte[] ScaledImage { get; private set; }
        public double Coefficient { get; private set; }
        public string ScaledImageString => Convert.ToBase64String(ScaledImage);
        public async Task<ActionResult> OnGetAsync(int imageId, int recordId)
        {
            ImageId = imageId;
            RecordId = recordId;
            if(imageId == 0)
            {
                return Redirect("Images");
            }
            var imageData = await _context.FetchFullImageById(ImageId);
            if(imageData == null)
            {
                return Redirect("Images");
            }

            (int fullImageWidth, int fullImageHeight) = ImageHelper.ImageSize(imageData);
            if(fullImageWidth > fullImageHeight)
            {
                if (fullImageWidth > MaxScaledWidth)
                {
                    Coefficient = fullImageWidth / (double)MaxScaledWidth;
                    ScaledImage = ImageHelper.ScaleImage(imageData, MaxScaledWidth);
                }
                else
                {
                    Coefficient = 1.0;
                    ScaledImage = imageData;
                }
            }
            else
            {
                if (fullImageHeight > MaxScaledHeight)
                {
                    Coefficient = fullImageHeight / (double)MaxScaledHeight;
                    ScaledImage = ImageHelper.ScaleImage(imageData, MaxScaledHeight);
                }
                else
                {
                    Coefficient = 1.0;
                    ScaledImage = imageData;
                }
            }
            return Page();
        }

        public async Task<RedirectToPageResult> OnPostCropImageAsync(int imageId, int selLeft, int selTop, int selWidth, int selHeight, double coefficient, int recordId)
        {
            RecordId = recordId;
            ImageId = imageId;
            if (selLeft >= 0 && selTop >= 0 && selWidth > 0 && selHeight > 0 && coefficient >= 1)
            {
                var image = await _context.FetchImageById(imageId);
                var sourceImage = await _context.FetchFullImageById(imageId);

                int realTop = Convert.ToInt32(selTop * coefficient);
                int realLeft = Convert.ToInt32(selLeft * coefficient);
                int realWidth = Convert.ToInt32(selWidth * coefficient);
                int realHeight = Convert.ToInt32(selHeight * coefficient);

                var tmpImage = ImageHelper.CropImage(image, sourceImage, realLeft, realTop, realWidth, realHeight);
                await _context.AddUnsavedTempImage(tmpImage);
            }
            return RedirectToPage("Edit", new { ImageId, RecordId });
        }
    }
}