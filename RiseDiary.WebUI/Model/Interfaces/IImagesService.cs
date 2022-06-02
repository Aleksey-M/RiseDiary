using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Model
{
    public interface IImagesService
    {
        Task<Guid> AddImage(IFormFile formFile, string imageName = "", int? newBiggestDimensionSize = null);

        Task DeleteImage(Guid imageId);

        Task<byte[]> FetchFullImageById(Guid imageId);

        Task<DiaryImage> FetchImageById(Guid imageId);

        Task<List<DiaryImage>> FetchImageSet(int skip, int count, string? imageNameFilter = null, Guid? recordId = null);

        Task<int> GetImagesCount(string? imageNameFilter = null, Guid? recordId = null);

        Task UpdateImage(Guid imageId, string imageNewName);
    }

    public class ImageNotFoundException : Exception
    {
        public ImageNotFoundException(Guid imageId) : base($"Image with id '{imageId}' does not exist") { }
    }
}
