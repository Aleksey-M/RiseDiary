using System.Collections.Generic;
using System.Threading.Tasks;
using RiseDiary.Domain.Model;

namespace RiseDiary.Domain.Repositories
{
    public interface IDiaryImagesStorage
    {
        Task<int> AddImage(DiaryImage image, byte[] imageData);
        Task DeleteImage(int imageId);
        Task<DiaryImage> GetImage(int imageId);
        Task<byte[]> GetImageData(int imageId);
        Task<List<DiaryImage>> GetImages(int skip, int count);
        Task<int> GetImagesCount();
        Task UpdateImageName(int imageId, string imageNewName);
    }
}