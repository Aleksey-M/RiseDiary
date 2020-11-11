using Microsoft.EntityFrameworkCore;
using RiseDiary.Shared;
using RiseDiary.WebUI.Data;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace RiseDiary.Model.Services
{
    public class CropImageService : ImagesEditService, ICropImageService
    {
        public CropImageService(DiaryDbContext context, IAppSettingsService appSettingsService) : base(context, appSettingsService) { }

        public async Task<ScaledImagePreview> CreateScaledImagePreview(Guid imageId)
        {
            var fullImage = await _context.FullSizeImages.AsNoTracking().SingleOrDefaultAsync(i => i.ImageId == imageId).ConfigureAwait(false);
            if (fullImage == null) throw new ArgumentException($"Image with Id='{imageId}' does not exists");

            int maxScaledWidth = await _appSettings.GetAppSettingInt(AppSettingsKey.CropImageMaxScaledWidth) ?? throw new Exception("Setting Value CropImageMaxScaledWidth not set");
            int maxScaledHeight = await _appSettings.GetAppSettingInt(AppSettingsKey.CropImageMaxScaledHeight) ?? throw new Exception("Setting Value CropImageMaxScaledHeight not set");
            int imageQuality = await _appSettings.GetAppSettingInt(AppSettingsKey.ImageQuality) ?? throw new Exception("Setting Value ImageQuality not set");

            return CreateScaledImagePreview(maxScaledWidth, maxScaledHeight, fullImage.Data, imageQuality);
        }

        public async Task CropImage(Guid imageId, Rectangle selectedPreviewRect, double scaleCoefficient)
        {
            if (scaleCoefficient < 1.0) throw new ArgumentException("Coefficient should be greater than 1");
            if (selectedPreviewRect.Left < 0 || selectedPreviewRect.Top < 0 || selectedPreviewRect.Width <= 0 || selectedPreviewRect.Height <= 0) throw new ArgumentException("Incorrect selection");

            var image = await _context.Images
                .AsNoTracking()
                .Include(i => i.FullImage)
                .Include(i => i.TempImage)
                .SingleOrDefaultAsync(i => i.Id == imageId)
                .ConfigureAwait(false);

            if (image.TempImage != null) throw new ArgumentException($"Image with id='{imageId}' has unsaved changes");

            int top = Convert.ToInt32(selectedPreviewRect.Top * scaleCoefficient);
            int left = Convert.ToInt32(selectedPreviewRect.Left * scaleCoefficient);
            int width = Convert.ToInt32(selectedPreviewRect.Width * scaleCoefficient);
            int height = Convert.ToInt32(selectedPreviewRect.Height * scaleCoefficient);

            if (top + height > image.Height) height = image.Height - top;
            if (left + width > image.Width) width = image.Width - left;

            int imageQuality = await _appSettings.GetAppSettingInt(AppSettingsKey.ImageQuality) ?? throw new Exception("Setting Value ImageQuality not set");
            if (image.FullImage == null) throw new Exception($"Full image for Id='{imageId}' is not found! Database is inconsist");

            var result = base.CropImage(image.FullImage.Data, left, top, width, height, imageQuality);
            var tmpImage = new TempImage
            {
                SourceImageId = image.Id,
                Modification = "Обрезка изображения",
                Data = result,
                SizeByte = result.Length
            };
            (tmpImage.Width, tmpImage.Height) = GetImageSize(result);

            await SaveModifiedImage(tmpImage);
        }

        protected ScaledImagePreview CreateScaledImagePreview(int maxScaledWidth, int maxScaledHeight, byte[] fullImage, int imageQuality)
        {
            (int fullImageWidth, int fullImageHeight) = GetImageSize(fullImage);

            var (coef, img) = (fullImageWidth > fullImageHeight, fullImageWidth > maxScaledWidth, fullImageHeight > maxScaledHeight, fullImageHeight > maxScaledHeight) switch
            {
                (true, true, _, _) => (fullImageWidth / (double)maxScaledWidth, ScaleImage(fullImage, imageQuality, maxScaledWidth)),
                (_, _, true, true) => (fullImageHeight / (double)maxScaledHeight, ScaleImage(fullImage, imageQuality, maxScaledHeight)),
                (_, _, _, _) => (1.0, fullImage)
            };

            return new ScaledImagePreview(coef, img);
        }
    }
}
