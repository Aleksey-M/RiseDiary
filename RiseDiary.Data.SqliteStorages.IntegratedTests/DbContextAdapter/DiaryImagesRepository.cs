using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RiseDiary.Data.SqliteStorages.IntegratedTests.Repositories;
using RiseDiary.Data.SqliteStorages.IntegratedTests.TestDomain;
using RiseDiary.WebUI.Data;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests.DbContextAdapter
{
    public class DiaryImagesRepository : IDiaryImagesRepository
    {
        private readonly DiaryDbContext _context;
        public DiaryImagesRepository(DiaryDbContext context)
        {
            _context = context;
        }

        public Task<int> AddImage(DiaryImage image, byte[] imageData)
        {
            var img = new Model.DiaryImage
            {
                Id = image.ImageId,
                CreateDate = image.CreateDate,
                Data = imageData,
                Name = image.ImageName
            };
            return _context.AddImage(img);
        }

        public Task AddImageForRecord(int recordId, int imageId)
        {
            return _context.AddImageForRecord(recordId, imageId);
        }

        public Task DeleteImage(int imageId)
        {
            return _context.DeleteImage(imageId);
        }

        public async Task<DiaryImage> FetchImageById(int imageId)
        {
            var i1 = await _context.FetchImageById(imageId);
            return i1 == null ? null : new DiaryImage
            {
                ImageId = i1.Id,
                CreateDate = i1.CreateDate,
                ImageName = i1.Name
            };
        }

        public Task<byte[]> FetchImageDataById(int imageId)
        {
            return _context.FetchImageDataById(imageId);            
        }

        public async Task<List<DiaryImage>> FetchImageSet(int skip = 0, int count = 10)
        {
            var list = await _context.FetchImageSet(skip, count);
            return list.Select(i => new DiaryImage
            {
                ImageId = i.Id,
                CreateDate = i.CreateDate,
                ImageName = i.Name
            }).ToList();
        }

        public async Task<List<DiaryImage>> FetchImagesForRecord(int recordId)
        {
            var list = await _context.FetchImagesForRecord(recordId);
            return list.Select(i => new DiaryImage
            {
                ImageId = i.Id,
                CreateDate = i.CreateDate,
                ImageName = i.Name
            }).ToList();
        }

        public Task<int> GetImagesCount()
        {
            return _context.GetImagesCount();
        }

        public Task<int> GetLinkedRecordsCount(int imageId)
        {
            return _context.GetLinkedRecordsCount(imageId);
        }

        public Task RemoveImageForRecord(int recordId, int imageId)
        {
            return _context.RemoveImageForRecord(recordId, imageId);
        }

        public Task UpdateImageName(int imageId, string imageNewName)
        {
            return _context.UpdateImageName(imageId, imageNewName);
        }
    }
}
