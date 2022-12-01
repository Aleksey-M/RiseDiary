namespace RiseDiary.Model;

public interface IImagesService
{
    Task<Guid> AddImage(IFormFile formFile, string imageName = "", int? newBiggestDimensionSize = null,
        string? cameraModel = null, DateTime? taken = null, string? contentType = null);

    Task DeleteImage(Guid imageId);

    Task<byte[]> FetchFullImageById(Guid imageId, CancellationToken cancellationToken = default);

    Task<DiaryImage> FetchImageById(Guid imageId, CancellationToken cancellationToken = default);

    Task<List<DiaryImage>> FetchImageSet(int skip, int count,
        string? imageNameFilter = null, Guid? recordId = null, CancellationToken cancellationToken = default);

    Task<int> GetImagesCount(string? imageNameFilter = null,
        Guid? recordId = null, CancellationToken cancellationToken = default);

    Task UpdateImage(Guid imageId, string imageNewName);
}

public class ImageNotFoundException : Exception
{
    public ImageNotFoundException(Guid imageId) : base($"Image with id '{imageId}' does not exist") { }
}
