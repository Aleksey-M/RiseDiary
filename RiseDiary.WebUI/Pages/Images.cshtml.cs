using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using RiseDiary.WebUI.Data;
using RiseDiary.Model;

namespace RiseDiary.WebUI.Pages
{
    public class ImagesPageModel : PageModel
    {
        private readonly DiaryDbContext _context;
        private readonly ILogger<ImagesPageModel> _logger;
        public ImagesPageModel(DiaryDbContext context, ILogger<ImagesPageModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        private IEnumerable<DiaryImage> _diaryImages;
        public IEnumerable<DiaryImage> DiaryImages { get => _diaryImages; }
        private string _currentImageBase64 = string.Empty;
        public string CurrentImage { get => _currentImageBase64; }

        private async Task UpdatePageState()
        {
            int imagesCount = await _context.GetImagesCount();
            _diaryImages = await _context.FetchImageSet(0, imagesCount);
        }

        public async Task OnGetAsync()
        {
            await UpdatePageState();
        }

        public async Task OnPostAddNewImageAsync(IFormFile newImage, string newImageName)
        {
            var validationErrors = new List<string>();
            if (newImage == null) validationErrors.Add("Файл изображения не выбран");
            if (string.IsNullOrWhiteSpace(newImageName)) validationErrors.Add("Название картинки не ведено");
            if (validationErrors.Count > 0)
            {
                validationErrors.ForEach(s => ModelState.AddModelError("Image", s));
                await UpdatePageState();
                return;
            }

            byte[] imageData = null;
            using (var binaryReader = new BinaryReader(newImage.OpenReadStream()))
            {
                imageData = binaryReader.ReadBytes((int)newImage.Length);
            }
            var img = new DiaryImage { CreateDate = DateTime.Now, Name = newImageName, Data = imageData };
            await _context.AddImage(img);

            await UpdatePageState();
        }

        public async Task OnPostImageViewAsync(int imageId)
        {
            var image = await _context.FetchImageById(imageId);
            if(image == null)
            {
                ModelState.AddModelError("Image", $"Изображение с id={imageId} не найдено");
            }
            else
            {
                _currentImageBase64 = Convert.ToBase64String(image.Data);
            }
            await UpdatePageState();
        }

        public async Task OnPostDeleteImageAsync(int imageId)
        {
            int recCount = await _context.GetLinkedRecordsCount(imageId);
            if (recCount > 0)
            {
                ModelState.AddModelError("Image", $"Изображение не может быть удалено, потому что добавлено к записям ({recCount})");
            }
            else
            {
                await _context.DeleteImage(imageId);
            }
            await UpdatePageState();
        }

        public async Task OnPostUpdateImageNameAsync(int imageId, string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
            {
                ModelState.AddModelError("Image", "Не введено новое название картинки");                
            }
            else
            {                
                await _context.UpdateImageName(imageId, imageName);
            }
            await UpdatePageState();            
        }
    }
}