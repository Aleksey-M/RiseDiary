using System.Drawing;

namespace RiseDiary.Model;

public enum Turn { Left, Right }

public interface IImagesEditService
{
    Task<bool> ImageHasChanges(Guid imageId);

    Task<TempImage> FetchChangedImage(Guid imageId);

    Task ApplyChanges(Guid imageId);

    Task DiscardChanges(Guid imageId);

    Task ReplaceImage(IFormFile newImage, Guid imageId, string? contentType = null);

    Task ReduceImageSize(Guid imageId, int newBiggestDimensionSize);

    Task<Guid> CreateNewImageFromChanged(Guid imageId);

    Task RotateImage(Guid imageId, Turn direction);

    Task CropImage(Guid imageId, Rectangle selectedPreviewRect);
}
