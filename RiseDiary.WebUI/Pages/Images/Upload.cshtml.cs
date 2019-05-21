using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Pages.Images
{
    public class UploadModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public UploadModel(DiaryDbContext context)
        {
            _context = context;
        }

        public int? TargetRecordId { get; private set; }

        public void OnGet(int? targetRecordId)
        {
            TargetRecordId = targetRecordId;
        }

        public async Task<IActionResult> OnPostAddNewImageAsync(List<IFormFile> newImages, string newImageName, int? targetRecordId)
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
            byte[] imageData = null;
            int imageId = 0;

            for (int i = 0; i < newImages.Count; i++)
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

                imageId = await _context.AddImage(imageName, imageData);
                if(TargetRecordId != null && TargetRecordId != 0)
                {
                    await _context.AddRecordImage(targetRecordId.Value, imageId);
                }                
            }

            if(newImages.Count == 1)
            {
                if (TargetRecordId != null && TargetRecordId != 0)
                    return Redirect($"/Images/Edit?recordId={TargetRecordId.Value}&imageId={imageId}");
                else
                    return Redirect($"/Images/Edit?recordId=0&imageId={imageId}");                
            }


            if (TargetRecordId != null && TargetRecordId != 0)
                return Redirect($"/Records/View?recordId={TargetRecordId.Value}");
            else
                return Redirect($"/Images");            
        }
    }
}