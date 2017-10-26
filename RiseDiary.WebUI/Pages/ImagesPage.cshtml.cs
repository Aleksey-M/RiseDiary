using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Domain.Repositories;
using Microsoft.Extensions.Logging;
using RiseDiary.Domain.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;

namespace RiseDiary.WebUI.Pages
{
    public class ImagesPageModel : PageModel
    {
        private readonly IRepositoriesFactory _repoFactory;
        private readonly ILogger<ImagesPageModel> _logger;
        public ImagesPageModel(IRepositoriesFactory factory, ILogger<ImagesPageModel> logger)
        {
            _repoFactory = factory;
            _logger = logger;
        }

        private IEnumerable<DiaryImage> _diaryImages;
        public IEnumerable<DiaryImage> DiaryImages { get => _diaryImages; }
        private string _currentImageBase64 = string.Empty;
        public string CurrentImage { get => _currentImageBase64; }

        private async Task UpdatePageState()
        {
            int imagesCount = await _repoFactory.DiaryImagesRepository.GetImagesCount();
            _diaryImages = await _repoFactory.DiaryImagesRepository.FetchImageSet(0, imagesCount);
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
            var img = new DiaryImage { CreateDate = DateTime.Now, ImageName = newImageName };
            await _repoFactory.DiaryImagesRepository.AddImage(img, imageData);

            await UpdatePageState();
        }

        public async Task OnPostImageViewAsync(int imageId)
        {
            var image = await _repoFactory.DiaryImagesRepository.FetchImageById(imageId);
            if(image == null)
            {
                ModelState.AddModelError("Image", $"Изображение с id={imageId} не найдено");
            }
            else
            {
                var imgBytes = await _repoFactory.DiaryImagesRepository.FetchImageDataById(imageId);
                _currentImageBase64 = Convert.ToBase64String(imgBytes);
            }
            await UpdatePageState();
        }

        public async Task OnPostDeleteImageAsync(int imageId)
        {
            await _repoFactory.DiaryImagesRepository.DeleteImage(imageId);
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
                await _repoFactory.DiaryImagesRepository.UpdateImageName(imageId, imageName);
            }
            await UpdatePageState();            
        }
    }
}