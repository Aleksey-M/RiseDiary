using Microsoft.EntityFrameworkCore;
using RiseDiary.Data;
using RiseDiary.Model;
using RiseDiary.WebAPI.Settings;
using RiseDiary.WebAPI.Settings.Model;

namespace RiseDiary.WebAPI.Services;

public class ImagesService : SkiaImageHandler, IImagesService
{
    protected readonly DiaryDbContext _context;

    protected readonly ISettingsService _appSettings;

    public ImagesService(DiaryDbContext context, ISettingsService appSettingsService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _appSettings = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
    }

    public async Task<Guid> AddImage(IFormFile formFile, string imageName = "", int? newBiggestDimensionSize = null,
        string? cameraModel = null, DateTime? taken = null, string? contentType = null)
    {
        if (string.IsNullOrWhiteSpace(imageName))
        {
            imageName = Path.GetFileNameWithoutExtension(formFile.FileName);
        }

        if (imageName.Length > 250) throw new ArgumentException("Image name length not should be more than 250 characters");

        using var ms = new MemoryStream();
        await formFile.CopyToAsync(ms).ConfigureAwait(false);

        var image = await AddImageWithoutSaving(ms.ToArray(), imageName, newBiggestDimensionSize, cameraModel, taken, contentType)
            .ConfigureAwait(false);

        await _context.SaveChangesAsync().ConfigureAwait(false);

        return image.Id;
    }

    protected async Task<ImageEntity> AddImageWithoutSaving(byte[] image, string imageName, int? newBiggestDimensionSize = null,
        string? cameraModel = null, DateTime? taken = null, string? contentType = null)
    {
        if (string.IsNullOrWhiteSpace(imageName)) throw new ArgumentException("Image Name should not be empty");

        ArgumentNullException.ThrowIfNull(image);
        ArgumentException.ThrowIfNullOrEmpty(contentType);

        var settings = await _appSettings.GetSetting<ImagesSettings>();
        var (file_taken, file_cameraModel) = GetMetadataFromPhoto(image);
        var (width, height) = GetImageSize(image);

        if (newBiggestDimensionSize is not null)
        {
            if (width > newBiggestDimensionSize || height > newBiggestDimensionSize)
            {
                image = ScaleImage(
                    image,
                    settings.Data?.ImageQuality
                        ?? throw new Exception("Setting Value ImageQuality not set"), newBiggestDimensionSize.Value);
                (width, height) = GetImageSize(image);
            }
        }

        var dImage = new ImageEntity
        {
            Id = Guid.NewGuid(),
            Name = imageName,
            CreateDate = DateTime.UtcNow,
            ModifyDate = DateTime.UtcNow,
            SizeByte = image.Length,
            Thumbnail = ScaleImage(
                image,
                settings.Data!.ImageQuality,
                settings.Data?.ThumbnailSize
                    ?? throw new Exception("Setting Value ThumbnailSize not set")),
            Taken = taken ?? file_taken,
            CameraModel = cameraModel ?? file_cameraModel,
            Height = height,
            Width = width,
            ContentType = contentType,
            Image = image
        };

        await _context.Images.AddAsync(dImage).ConfigureAwait(false);

        return dImage;
    }

    public async Task DeleteImage(Guid imageId)
    {
        var img = await _context.Images
            .Include(i => i.RecordsRefs)
            .Include(i => i.TempImage)
            .FirstOrDefaultAsync(i => i.Id == imageId)
            .ConfigureAwait(false);

        if (img != null)
        {
            _context.Images.Remove(img);

            foreach (var recordImage in img.RecordsRefs)
            {
                var nextRecordImages = await _context.RecordImages
                    .Where(x => x.RecordId == recordImage.RecordId && x.Order >= recordImage.Order)
                    .ToListAsync().ConfigureAwait(false);

                if (nextRecordImages.Count > 0)
                {
                    nextRecordImages.ForEach(x => x.Order--);
                }
            }

            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public async Task UpdateImage(Guid imageId, string imageNewName)
    {
        if (string.IsNullOrWhiteSpace(imageNewName)) throw new ArgumentException("Image name should not be empty");
        if (imageNewName.Length > 250) throw new ArgumentException("Image name length not should be more than 250 characters");

        var img = await _context.Images.SingleOrDefaultAsync(i => i.Id == imageId).ConfigureAwait(false);
        _ = img ?? throw new ImageNotFoundException(imageId);

        img.Name = imageNewName;
        img.ModifyDate = DateTime.UtcNow;
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<ImageEntity> FetchImageById(Guid imageId, CancellationToken cancellationToken)
    {
        var img = await _context.Images
            .Include(i => i.TempImage)
            .AsNoTracking()
            .SingleOrDefaultAsync(i => i.Id == imageId, cancellationToken)
            .ConfigureAwait(false);

        return img ?? throw new ImageNotFoundException(imageId);
    }

    public async Task<byte[]> FetchFullImageById(Guid imageId, CancellationToken cancellationToken)
    {
        var img = await _context.Images
            .AsNoTracking()
            .Include(i => i.TempImage)
            .SingleOrDefaultAsync(i => i.Id == imageId, cancellationToken)
            .ConfigureAwait(false);

        _ = img ?? throw new ImageNotFoundException(imageId);

        if (img.TempImage != null) return img.TempImage.Data;
        return img.Image ?? throw new Exception("Saved image is not contains image data");
    }

    public async Task<int> GetImagesCount(string? imageNameFilter, Guid? recordId, CancellationToken cancellationToken)
    {
        var nameFilter = imageNameFilter?.Trim()?.ToUpper();

        if (string.IsNullOrEmpty(nameFilter))
        {
            return await _context.Images
               .Where(x => !x.RecordsRefs.Any(y => y.RecordId == recordId))
               .CountAsync(cancellationToken)
               .ConfigureAwait(false);
        }

        var imagesNames = await _context.Images
            .Where(x => !x.RecordsRefs.Any(y => y.RecordId == recordId))
            .Select(x => x.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return imagesNames
            .Count(x => x.Contains(nameFilter, StringComparison.CurrentCultureIgnoreCase));
    }

    public async Task<List<ImageEntity>> FetchImageSet(int skip, int count,
        string? imageNameFilter, Guid? recordId, CancellationToken cancellationToken)
    {
        var nameFilter = imageNameFilter?.Trim();

        if (string.IsNullOrEmpty(nameFilter))
        {
            return await _context.Images
                .Where(x => !x.RecordsRefs.Any(y => y.RecordId == recordId))
                .OrderByDescending(i => i.CreateDate)
                .Skip(skip)
                .Take(count)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        var imagesNames = await _context.Images
           .Where(x => !x.RecordsRefs.Any(y => y.RecordId == recordId))
           .Select(x => new { x.Id, x.Name })
           .ToListAsync(cancellationToken)
           .ConfigureAwait(false);

        var imagesIds = imagesNames
             .Where(x => x.Name.Contains(nameFilter, StringComparison.CurrentCultureIgnoreCase))
             .Select(x => x.Id)
             .ToList();

        return await _context.Images
            .Where(x => imagesIds.Contains(x.Id))
            .Where(x => !x.RecordsRefs.Any(y => y.RecordId == recordId))
            .OrderByDescending(i => i.CreateDate)
            .Skip(skip)
            .Take(count)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

}
