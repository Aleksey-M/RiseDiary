using System.Drawing;
using Microsoft.EntityFrameworkCore;
using RiseDiary.Data;
using RiseDiary.Shared;

namespace RiseDiary.Model.Services;

internal class ImagesEditService : ImagesService, IImagesEditService
{
    public ImagesEditService(DiaryDbContext context, IAppSettingsService appSettingsService) : base(context, appSettingsService) { }

    public async Task ApplyChanges(Guid imageId)
    {
        var image = await _context.Images
            .Include(i => i.TempImage)
            .Include(i => i.FullImage)
            .SingleOrDefaultAsync(i => i.Id == imageId)
            .ConfigureAwait(false);

        if (image == null) throw new ArgumentException($"Image with Id='{imageId}' does not exists");
        if (image.TempImage == null) throw new ArgumentException($"Image with Id='{imageId}' does not have any changes to apply");
        if (image.FullImage == null) throw new Exception($"Full image for Id='{imageId}' is not found! Database is inconsist");
        if (image.Deleted) throw new ArgumentException("Image is deleted. Can't update deleted image");

        int imageQuality = await _appSettings.GetAppSettingInt(AppSettingsKey.ImageQuality) ?? throw new Exception("Setting Value ImageQuality not set");
        int thumbnailSize = await _appSettings.GetAppSettingInt(AppSettingsKey.ThumbnailSize) ?? throw new Exception("Setting Value ThumbnailSize not set");

        image.Thumbnail = ScaleImage(image.TempImage.Data, imageQuality, thumbnailSize);
        image.ModifyDate = DateTime.UtcNow;
        image.SizeByte = image.TempImage.Data.Length;
        (image.Width, image.Height) = GetImageSize(image.TempImage.Data);
        image.ContentType = "image/jpeg";
        image.FullImage.Data = image.TempImage.Data;
        image.ContentType = image.TempImage.ContentType;

        _context.TempImages.Remove(image.TempImage);

        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<Guid> CreateNewImageFromChanged(Guid imageId)
    {
        var image = await _context.Images
            .Include(i => i.TempImage)
            .SingleOrDefaultAsync(i => i.Id == imageId)
            .ConfigureAwait(false);

        if (image == null) throw new ArgumentException($"Image with Id='{imageId}' does not exists");
        if (image.TempImage == null) throw new ArgumentException($"Image with Id='{imageId}' does not have any changes to apply");

        var newImage = await AddImageWithoutSaving(
            image.TempImage.Data, 
            $"{image.Name} ({image.TempImage.Modification})",
            null,
            image.CameraModel,
            image.Taken,
            image.TempImage.ContentType);

        _context.TempImages.Remove(image.TempImage);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        return newImage.Id;
    }

    public async Task DiscardChanges(Guid imageId)
    {
        var oldImages = _context.TempImages.Where(t => t.SourceImageId == imageId);
        if (oldImages.Any())
        {
            _context.TempImages.RemoveRange(oldImages);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public async Task<TempImage> FetchChangedImage(Guid imageId) => await _context.TempImages
        .AsNoTracking()
        .FirstOrDefaultAsync(t => t.SourceImageId == imageId)
        .ConfigureAwait(false) ?? throw new ArgumentException($"Image with id='{imageId}' does not have any changes");

    public async Task<bool> ImageHasChanges(Guid imageId) => await _context.TempImages
        .AsNoTracking()
        .AnyAsync(t => t.SourceImageId == imageId)
        .ConfigureAwait(false);

    public async Task ReduceImageSize(Guid imageId, int newBiggestDimensionSize)
    {
        if (newBiggestDimensionSize == 0) throw new ArgumentException(null, nameof(newBiggestDimensionSize));
        var image = await _context.Images.SingleOrDefaultAsync(i => i.Id == imageId).ConfigureAwait(false);
        if (image == null) throw new ArgumentException($"Image with Id='{imageId}' does not exists");

        bool changesAlreadyExists = await ImageHasChanges(imageId);
        if (changesAlreadyExists) throw new ArgumentException($"Image with id='{imageId}' has unsaved changes");

        if (image.GetBiggestImageDimm() < newBiggestDimensionSize) throw new Exception("New dimension should be less than actual");

        var sourceImage = await _context.FullSizeImages
            .AsNoTracking()
            .SingleOrDefaultAsync(i => i.ImageId == imageId)
            .ConfigureAwait(false) ?? throw new Exception($"Full size image with Id = '{imageId}' does not exists");

        int imageQuality = await _appSettings.GetAppSettingInt(AppSettingsKey.ImageQuality) ?? throw new Exception("Setting Value ImageQuality not set");

        var result = ScaleImage(sourceImage.Data, imageQuality, newBiggestDimensionSize);

        var temp = new TempImage
        {
            SourceImageId = image.Id,
            Modification = "Сжатие изображения",
            Data = result,
            SizeByte = result.Length,
            ContentType = "image/jpeg"
        };

        (temp.Width, temp.Height) = GetImageSize(result);

        await SaveModifiedImage(temp).ConfigureAwait(false);
    }

    public async Task ReplaceImage(IFormFile newImage, Guid imageId, string? contentType = null)
    {
        ArgumentNullException.ThrowIfNull(newImage);

        var image = await _context.Images.SingleOrDefaultAsync(i => i.Id == imageId);
        if (image == null) throw new ArgumentException($"Image with Id='{imageId}' does not exists");

        bool changesAlreadyExists = await ImageHasChanges(imageId);
        if (changesAlreadyExists) throw new Exception($"Image with id='{imageId}' has unsaved changes");

        using var ms = new MemoryStream();
        await newImage.CopyToAsync(ms).ConfigureAwait(false);
        var imageArray = ms.ToArray();

        var temp = new TempImage
        {
            SourceImageId = image.Id,
            Modification = "Замена изображения",
            Data = imageArray,
            SizeByte = imageArray.Length,
            ContentType = contentType ?? newImage.ContentType
        };

        (temp.Width, temp.Height) = GetImageSize(imageArray);

        await SaveModifiedImage(temp).ConfigureAwait(false);
    }

    public async Task RotateImage(Guid imageId, Turn direction)
    {
        var image = await _context.Images
            .SingleOrDefaultAsync(i => i.Id == imageId)
            .ConfigureAwait(false);

        if (image == null) throw new ArgumentException($"Image with Id='{imageId}' does not exists");

        bool changesAlreadyExists = await ImageHasChanges(imageId);
        if (changesAlreadyExists) throw new ArgumentException($"Image with id='{imageId}' has unsaved changes");

        var sourceImage = await _context.FullSizeImages
            .AsNoTracking()
            .SingleOrDefaultAsync(i => i.ImageId == imageId)
            .ConfigureAwait(false) ?? throw new Exception($"Full size image with Id = '{imageId}' does not exists");

        int imageQuality = await _appSettings.GetAppSettingInt(AppSettingsKey.ImageQuality) ?? throw new Exception("Setting Value ImageQuality not set");

        var result = direction switch
        {
            Turn.Left => RotateImage(sourceImage.Data, 270, imageQuality),
            Turn.Right => RotateImage(sourceImage.Data, 90, imageQuality),
            _ => throw new Exception("Unknown direction")
        };

        var temp = new TempImage
        {
            SourceImageId = image.Id,
            Modification = direction switch
            {
                Turn.Left => "Поворот на 90 градусов влево",
                Turn.Right => "Поворот на 90 градусов вправо",
                _ => ""
            },
            Data = result,
            SizeByte = result.Length,
            ContentType = "image/jpeg"
        };

        (temp.Width, temp.Height) = GetImageSize(result);

        await SaveModifiedImage(temp).ConfigureAwait(false);
    }

    protected async Task SaveModifiedImage(TempImage image)
    {
        var oldImages = _context.TempImages.Where(t => t.SourceImageId == image.SourceImageId);

        if (oldImages.Any())
        {
            _context.TempImages.RemoveRange(oldImages);
        }

        await _context.TempImages.AddAsync(image);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task CropImage(Guid imageId, Rectangle selectedPreviewRect)
    {
        if (selectedPreviewRect.Left < 0 
            || selectedPreviewRect.Top < 0
            || selectedPreviewRect.Width <= 0
            || selectedPreviewRect.Height <= 0) throw new ArgumentException("Incorrect selection");

        var image = await _context.Images
            .AsNoTracking()
            .Include(i => i.FullImage)
            .Include(i => i.TempImage)
            .SingleOrDefaultAsync(i => i.Id == imageId)
            .ConfigureAwait(false);

        if (image is null) throw new ArgumentException($"Image with id='{imageId}' was not found");
        if (image.TempImage != null) throw new ArgumentException($"Image with id='{imageId}' has unsaved changes");

        int top = selectedPreviewRect.Top;
        int left = selectedPreviewRect.Left;
        int width = selectedPreviewRect.Width;
        int height = selectedPreviewRect.Height;

        if (top + height > image.Height) height = image.Height - top;
        if (left + width > image.Width) width = image.Width - left;

        int imageQuality = await _appSettings.GetAppSettingInt(AppSettingsKey.ImageQuality) ?? throw new Exception("Setting Value ImageQuality not set");
        if (image.FullImage == null) throw new Exception($"Full image for Id='{imageId}' is not found! Database is inconsist");

        var result = CropImage(image.FullImage.Data, left, top, width, height, imageQuality);

        var tmpImage = new TempImage
        {
            SourceImageId = image.Id,
            Modification = "Обрезка изображения",
            Data = result,
            SizeByte = result.Length,
            ContentType = "image/jpeg"
        };

        (tmpImage.Width, tmpImage.Height) = GetImageSize(result);

        await SaveModifiedImage(tmpImage);
    }
}
