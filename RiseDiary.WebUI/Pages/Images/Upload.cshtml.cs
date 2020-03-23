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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
    public class UploadModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public UploadModel(DiaryDbContext context)
        {
            _context = context;
        }

        public Guid? TargetRecordId { get; private set; }

        public void OnGet(Guid? targetRecordId)
        {
            TargetRecordId = targetRecordId;
        }

        public async Task<IActionResult> OnPostAddNewImageAsync(List<IFormFile> newImages, string newImageName, Guid? targetRecordId)
        {
            TargetRecordId = targetRecordId;
            var validationErrors = new List<string>();
            if (newImages == null || newImages.Count == 0) validationErrors.Add("Файл изображения не выбран");

            if (validationErrors.Count > 0)
            {
                validationErrors.ForEach(s => ModelState.AddModelError("Image", s));
                return Page();
            }

            string imageName = string.Empty;
            byte[] imageData = Array.Empty<byte>();
            Guid imageId = Guid.Empty;

            for (int i = 0; i < newImages?.Count; i++)
            {
                if(newImages.Count == 1)
                    imageName = string.IsNullOrWhiteSpace(newImageName) ?
                    Path.GetFileNameWithoutExtension(newImages[i].FileName) :
                    newImageName;
                else
                    imageName = string.IsNullOrWhiteSpace(newImageName) ? 
                    Path.GetFileNameWithoutExtension(newImages[i].FileName) : 
                    $"{newImageName} ({i + 1})";

                using (var binaryReader = new BinaryReader(newImages[i].OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)newImages[i].Length);
                }

                int imageQuality = await _context.GetAppSettingInt(AppSettingsKeys.ImageQuality) ?? 75;
                var (taken, cameraModel) = ImageHelper.GetMetadataFromPhoto(imageData);

                imageId = await _context.AddImage(imageName, imageData, imageQuality, taken: taken, cameraModel: cameraModel);
                if(TargetRecordId != null && TargetRecordId != Guid.Empty)
                {
                    await _context.AddRecordImage(TargetRecordId.Value, imageId);
                }                
            }

            if(newImages?.Count == 1)
            {
                if (TargetRecordId != null && TargetRecordId != Guid.Empty)
                    return Redirect($"/Images/Edit?recordId={TargetRecordId.Value}&imageId={imageId}");
                else
                    return Redirect($"/Images/Edit?recordId=0&imageId={imageId}");                
            }


            if (TargetRecordId != null && TargetRecordId != Guid.Empty)
                return Redirect($"/Records/View?recordId={TargetRecordId.Value}");
            else
                return Redirect($"/Images");            
        }
    }
}