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

        public async Task<IActionResult> OnPostAddNewImageAsync(IFormFile newImage, string newImageName, int? targetRecordId)
        {
            TargetRecordId = targetRecordId;
            var validationErrors = new List<string>();
            if (newImage == null) validationErrors.Add("Файл изображения не выбран");

            if (validationErrors.Count > 0)
            {
                validationErrors.ForEach(s => ModelState.AddModelError("Image", s));
                return Page();
            }

            string imageName = string.IsNullOrWhiteSpace(newImageName) ? Path.GetFileNameWithoutExtension(newImage.FileName) : newImageName;

            byte[] imageData = null;
            using (var binaryReader = new BinaryReader(newImage.OpenReadStream()))
            {
                imageData = binaryReader.ReadBytes((int)newImage.Length);
            }
            var imageId = await _context.AddImage(imageName, imageData);

            if(TargetRecordId != null && TargetRecordId != 0)
            {
                await _context.AddRecordImage(targetRecordId.Value, imageId);
                return Redirect($"/Images/Edit?recordId={TargetRecordId.Value}&imageId={imageId}");
            }
            return Redirect($"/Images/Edit?recordId=0&imageId={imageId}");
        }
    }
}