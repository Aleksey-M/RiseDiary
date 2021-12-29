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

        Task<List<DiaryImage>> FetchImageSet(int skip, int count);

        Task<int> GetImagesCount();

        Task UpdateImage(Guid imageId, string imageNewName);
    }
}
