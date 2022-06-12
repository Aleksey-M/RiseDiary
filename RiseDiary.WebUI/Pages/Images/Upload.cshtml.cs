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

        public async Task<IActionResult> OnPostAddNewImageAsync(List<IFormFile> newImages, string? newImageName, int? newBiggestDimension, Guid? targetRecordId)
        {
            TargetRecordId = targetRecordId;

            if (newImages == null || newImages.Count == 0)
            {
                ModelState.AddModelError("Image", "Файл изображения не выбран");
                return Page();
            }

            int? biggestDimmSize = newBiggestDimension;
            if (biggestDimmSize.HasValue && (biggestDimmSize > 10_000 || biggestDimmSize < 100))
            {
                biggestDimmSize = null;
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

                newImageId = await _imagesService.AddImage(newImages[i], newImgName, biggestDimmSize);

                if (HasRecord(TargetRecordId))
                {
                    await _recordImagesService.AddRecordImage(TargetRecordId!.Value, newImageId);
                }
            }

            return (newImages!.Count == 1, HasRecord(TargetRecordId)) switch
            {
                (true, true)  => Redirect($"/Images/Edit?recordId={TargetRecordId!.Value}&imageId={newImageId}"),
                (true, false) => Redirect($"/Images/Edit?imageId={newImageId}"),
                (false, true) => Redirect($"/Records/View?recordId={TargetRecordId!.Value}"),
                (false, false) => Redirect($"/Images")
            };

            static bool HasRecord(Guid? recordId) => recordId != null && recordId != Guid.Empty;
        }
    }
}