using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Images
{
    public class EditModel : PageModel
    {
        private readonly IImagesService _imagesService;
        private readonly IRecordsImagesService _recordImagesService;
        private readonly IImagesEditService _imagesEditService;

        public EditModel(IImagesService imagesService, IRecordsImagesService recordImagesService, IImagesEditService imagesEditService)
        {
            _imagesService = imagesService;
            _recordImagesService = recordImagesService;
            _imagesEditService = imagesEditService;
        }

        public Guid? RecordId { get; private set; }

        public DiaryImage Image { get; private set; } = null!;

        public Uri ImageUrl { get; private set; } = null!;

        public Dictionary<Guid, string> ImageLinks { get; private set; } = new Dictionary<Guid, string>();


        private async Task UpdateModel(Guid imageId)
        {
            string url = Request.GetAppBaseUrl();

            Image = await _imagesService.FetchImageById(imageId);
            ImageUrl = new Uri($@"{url}/api/v1.0/image-file/{Image.Id}");
            ImageLinks = await _recordImagesService.GetLinkedRecordsInfo(imageId);
        }

        public async Task<IActionResult> OnGetAsync(Guid? imageId, Guid? recordId)
        {
            if (!imageId.HasValue || imageId.Value == default)
            {
                return Redirect(recordId);
            }            

            RecordId = recordId;

            try
            {
                await UpdateModel(imageId.Value);
            }
            catch (ImageNotFoundException)
            {
                return Redirect(recordId);
            }

            return Page();

            IActionResult Redirect(Guid? recordId)
            {
                if (recordId.HasValue && recordId.Value != default)
                {
                    return base.Redirect("/records/view?recordid=" + recordId.Value);
                }
                return base.Redirect("/images/index");
            }
        }

        public async Task<IActionResult> OnPostDeleteImageAsync(Guid? imageId, Guid? recordId)
        {
            if (imageId != null && imageId.Value != Guid.Empty)
            {
                await _imagesService.DeleteImage(imageId.Value);
            }
            return (recordId != null && recordId != Guid.Empty) ? Redirect($"/Records/View?recordid={recordId.Value}") : Redirect("/images/index");
        }

        public async Task OnPostReplaceImageAsync(IFormFile newImage, Guid imageId, Guid? recordId)
        {
            if (imageId != Guid.Empty)
            {
                RecordId = recordId;
                if (newImage != null)
                {
                    await _imagesEditService.ReplaceImage(newImage, imageId);
                }
                await UpdateModel(imageId);
            }
        }

        public async Task OnPostCancelEditAsync(Guid imageId, Guid? recordId)
        {
            RecordId = recordId;
            await _imagesEditService.DiscardChanges(imageId);
            await UpdateModel(imageId);
        }

        public async Task OnPostSaveUpdatedImageAsync(Guid imageId, Guid? recordId)
        {
            RecordId = recordId;
            await _imagesEditService.ApplyChanges(imageId);
            await UpdateModel(imageId);
        }

        public async Task OnPostSaveUpdatedAsNewImageAsync(Guid imageId, Guid? recordId)
        {
            RecordId = recordId;
            var newImageId = await _imagesEditService.CreateNewImageFromChanged(imageId);

            if (RecordId != null && RecordId.Value != Guid.Empty)
            {
                await _recordImagesService.AddRecordImage(RecordId.Value, newImageId);
            }
            await UpdateModel(newImageId);
        }

        public async Task OnPostScaleImageAsync(Guid imageId, int imageSize, Guid? recordId)
        {
            RecordId = recordId;
            Image = await _imagesService.FetchImageById(imageId);
            if (imageSize > 0 && imageSize <= Image.GetBiggestImageDimm())
            {
                await _imagesEditService.ReduceImageSize(imageId, imageSize);
            }
            await UpdateModel(imageId);
        }

        public async Task OnPostRotateImageAsync(Guid imageId, Guid? recordId, string direction)
        {
            RecordId = recordId;
            Image = await _imagesService.FetchImageById(imageId);

            if (direction is { Length: > 0 })
            {
                if(direction.Equals("left", StringComparison.OrdinalIgnoreCase))
                {
                    await _imagesEditService.RotateImage(imageId, Turn.Left);
                }
                else
                {
                    await _imagesEditService.RotateImage(imageId, Turn.Right);
                }
            }            

            await UpdateModel(imageId);
        }

    }
}