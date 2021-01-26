using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Images
{
    public class UploadModel : PageModel
    {
        private readonly IRecordsImagesService _recordImagesService;
        private readonly IImagesService _imagesService;

        public UploadModel(IImagesService imagesService, IRecordsImagesService recordImagesService)
        {
            _recordImagesService = recordImagesService;
            _imagesService = imagesService;
        }

        public Guid? TargetRecordId { get; private set; }

        public void OnGet(Guid? recordId)
        {
            TargetRecordId = recordId;
        }

        public async Task<IActionResult> OnPostAddNewImageAsync(List<IFormFile> newImages, string? newImageName, Guid? targetRecordId)
        {
            TargetRecordId = targetRecordId;
            var validationErrors = new List<string>();
            if (newImages == null || newImages.Count == 0) validationErrors.Add("Файл изображения не выбран");

            if (validationErrors.Count > 0)
            {
                validationErrors.ForEach(s => ModelState.AddModelError("Image", s));
                return Page();
            }

            Guid newImageId = Guid.Empty;

            for (int i = 0; i < newImages?.Count; i++)
            {
                string newImgName = (string.IsNullOrWhiteSpace(newImageName), newImages.Count > 1) switch
                {
                    (true, _) => "",
                    (false, true) => $"{newImageName!} ({i + 1})",
                    (false, false) => newImageName!
                };

                newImageId = await _imagesService.AddImage(newImages[i], newImgName);

                if (TargetRecordId != null && TargetRecordId != Guid.Empty)
                {
                    await _recordImagesService.AddRecordImage(TargetRecordId.Value, newImageId);
                }
            }

            if (newImages?.Count == 1)
            {
                if (TargetRecordId != null && TargetRecordId != Guid.Empty)
                    return Redirect($"/Images/Edit?recordId={TargetRecordId.Value}&imageId={newImageId}");
                else
                    return Redirect($"/Images/Edit?recordId=0&imageId={newImageId}");
            }


            if (TargetRecordId != null && TargetRecordId != Guid.Empty)
                return Redirect($"/Records/View?recordId={TargetRecordId.Value}");
            else
                return Redirect($"/Images");
        }
    }
}