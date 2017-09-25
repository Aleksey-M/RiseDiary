using System.Collections.Generic;
using System.Threading.Tasks;
using RiseDiary.Domain.Model;

namespace RiseDiary.Domain.Repositories
{
    public interface IDiaryImagesRepository
    {
        Task<int> AddImage(DiaryImage image, byte[] imageData);
        Task DeleteImage(int imageId);
        Task<DiaryImage> FetchImageById(int imageId);
        Task<byte[]> FetchImageDataById(int imageId);
        Task<List<DiaryImage>> FetchImageSet(int skip = 0, int count = 10);
        Task<int> GetImagesCount();
        Task UpdateImageName(int imageId, string imageNewName);

        Task AddImageForRecord(int recordId, int imageId);
        Task RemoveImageForRecord(int recordId, int imageId);
        Task<List<DiaryImage>> FetchImagesForRecord(int recordId);
        Task<int> GetLinkedRecordsCount(int imageId);
    }
}