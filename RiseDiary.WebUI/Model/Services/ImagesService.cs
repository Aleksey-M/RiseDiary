using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RiseDiary.Shared;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.Model.Services
{
    public class ImagesService : SkiaImageHandler, IImagesService
    {
        protected readonly DiaryDbContext _context;
        protected readonly IAppSettingsService _appSettings;

        public ImagesService(DiaryDbContext context, IAppSettingsService appSettingsService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _appSettings = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        }

        public async Task<Guid> AddImage(IFormFile formFile, string imageName = "")
        {
            if (string.IsNullOrWhiteSpace(imageName))
            {
                imageName = Path.GetFileNameWithoutExtension(formFile.FileName);
            }

            if (imageName.Length > 250) throw new ArgumentException("Image name length not should be more than 250 characters");

            using var ms = new MemoryStream();
            await formFile.CopyToAsync(ms).ConfigureAwait(false);

            var image = await AddImageWithoutSaving(ms.ToArray(), imageName).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return image.Id;
        }

        protected async Task<DiaryImage> AddImageWithoutSaving(byte[] image, string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName)) throw new ArgumentException("Image Name should not be empty");
            _ = image ?? throw new ArgumentNullException(nameof(image));

            int imageQuality = await _appSettings.GetAppSettingInt(AppSettingsKey.ImageQuality) ?? throw new Exception("Setting Value ImageQuality not set");
            int thumbnailSize = await _appSettings.GetAppSettingInt(AppSettingsKey.ThumbnailSize) ?? throw new Exception("Setting Value ThumbnailSize not set");

            var (taken, cameraModel) = GetMetadataFromPhoto(image);

            var dImage = new DiaryImage
            {
                Id = Guid.NewGuid(),
                Name = imageName,
                CreateDate = DateTime.Now,
                ModifyDate = DateTime.Now,
                SizeByte = image.Length,
                Thumbnail = ScaleImage(image, imageQuality, thumbnailSize),
                Taken = taken,
                CameraModel = cameraModel,
                FullImage = new DiaryImageFull
                {
                    Data = image
                }
            };
            (dImage.Width, dImage.Height) = GetImageSize(image);
            await _context.Images.AddAsync(dImage).ConfigureAwait(false);

            return dImage;
        }

        public async Task DeleteImage(Guid imageId)
        {
            var img = await _context.Images
                .Include(i => i.RecordsRefs)
                .Include(i => i.FullImage)
                .Include(i => i.TempImage)
                .FirstOrDefaultAsync(i => i.Id == imageId)
                .ConfigureAwait(false);

            if (img != null)
            {
                _context.Images.Remove(img);
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task UpdateImage(Guid imageId, string imageNewName)
        {
            if (string.IsNullOrWhiteSpace(imageNewName)) throw new ArgumentException("Image name should not be empty");
            if (imageNewName.Length > 250) throw new ArgumentException("Image name length not should be more than 250 characters");

            var img = await _context.Images.SingleOrDefaultAsync(i => i.Id == imageId).ConfigureAwait(false);
            _ = img ?? throw new ArgumentException($"Image with id '{imageId}' does not exist");

            img.Name = imageNewName;
            img.ModifyDate = DateTime.Now;
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<DiaryImage> FetchImageById(Guid imageId)
        {
            var img = await _context.Images
                .Include(i => i.TempImage)
                .AsNoTracking()
                .SingleOrDefaultAsync(i => i.Id == imageId)
                .ConfigureAwait(false);

            return img ?? throw new ArgumentException($"Image with id '{imageId}' does not exist");
        }

        public async Task<byte[]> FetchFullImageById(Guid imageId)
        {
            var img = await _context.Images
                .AsNoTracking()
                .Include(i => i.FullImage)
                .Include(i => i.TempImage)
                .SingleOrDefaultAsync(i => i.Id == imageId).ConfigureAwait(false);
            _ = img ?? throw new ArgumentException($"Image with id '{imageId}' does not exist");

            if (img.TempImage != null) return img.TempImage.Data;

            return img.FullImage?.Data ?? throw new Exception("Saved image is not contains image data");
        }

        public async Task<int> GetImagesCount() => await _context.Images.AsNoTracking().CountAsync();

        public async Task<List<DiaryImage>> FetchImageSet(int skip, int count) =>
            await _context.Images
            .AsNoTracking()
            .OrderByDescending(i => i.CreateDate)
            .Skip(skip).Take(count)
            .ToListAsync()
            .ConfigureAwait(false);

    }
}
