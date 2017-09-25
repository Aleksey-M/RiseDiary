using Dapper;
using RiseDiary.Domain.Model;
using RiseDiary.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseDiary.Data.SqliteStorages
{
    public class DiaryImagesRepository : IDiaryImagesRepository
    {
        private readonly IDataBaseManager _manager;
        public DiaryImagesRepository(IDataBaseManager dbManager)
        {
            _manager = dbManager;
        }
        public async Task<int> AddImage(DiaryImage image, byte[] imageData)
        {
            if(image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }
            if(imageData == null || imageData.Length == 0)
            {
                throw new ArgumentException(nameof(image));
            }
            using (var connection = await _manager.GetConnection())
            using (var transaction = connection.BeginTransaction())
            {                
                try
                {
                    int newRecId = (await connection.QueryAsync<int>("INSERT INTO Images ( ImageName, CreateDate, Data ) VALUES ( @ImageName, @CreateDate, @Data ); SELECT last_insert_rowid() ", 
                        new { image.ImageName, image.CreateDate, Data = imageData })).First();                    
                    transaction.Commit();
                    return newRecId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        public async Task DeleteImage(int imageId)
        {
            using (var connection = await _manager.GetConnection())
            {
                await connection.ExecuteAsync("DELETE FROM Images WHERE ImageId = @imageId", new { imageId });
            }
        }
        public async Task UpdateImageName(int imageId, string imageNewName)
        {
            using (var connection = await _manager.GetConnection())
            {
                await connection.ExecuteAsync("UPDATE Images SET ImageName = @imageNewName WHERE ImageId = @imageId", new { imageId, imageNewName });
            }
        }
        public async Task<DiaryImage> FetchImageById(int imageId)
        {
            using (var connection = await _manager.GetConnection())
            {
                return (await connection.QueryAsync<DiaryImage>("SELECT ImageId, ImageName, CreateDate FROM Images WHERE ImageId = @imageId", new { imageId })).FirstOrDefault();
            }
        }
        public async Task<int> GetImagesCount()
        {
            using (var connection = await _manager.GetConnection())
            {
                return (await connection.QueryAsync<int>("SELECT COUNT(*) FROM Images")).First();
            }
        }
        public async Task<List<DiaryImage>> FetchImageSet(int skip, int count)
        {
            using (var connection = await _manager.GetConnection())
            {
                return (await connection.QueryAsync<DiaryImage>("SELECT ImageId, ImageName, CreateDate FROM Images ORDER BY CreateDate DESC LIMIT @skip, @count", new { skip, count })).ToList();
            }
        }
        public async Task<byte[]> FetchImageDataById(int imageId)
        {
            using (var connection = await _manager.GetConnection())
            {
                return (await connection.QueryAsync<byte[]>("SELECT Data FROM Images WHERE ImageId = @imageId", new { imageId })).FirstOrDefault();
            }
        }

        public async Task AddImageForRecord(int recordId, int imageId)
        {
            using (var connection = await _manager.GetConnection())
            {
                await connection.ExecuteAsync($"INSERT INTO ImagesOfRecord VALUES (@imageId, @recordId)", new { imageId, recordId});
            }
        }

        public async Task RemoveImageForRecord(int recordId, int imageId)
        {
            using (var connection = await _manager.GetConnection())
            {
                await connection.ExecuteAsync($"DELETE FROM ImagesOfRecord WHERE ImageId = @imageId AND RecordId = @recordId", new { imageId, recordId });
            }
        }

        public async Task<List<DiaryImage>> FetchImagesForRecord(int recordId)
        {
            using (var connection = await _manager.GetConnection())
            {
                var sql = new StringBuilder(
                    "SELECT Img.ImageId, Img.ImageName, Img.CreateDate FROM Images AS Img")
                    .Append("INNER JOIN ImagesOfRecord AS IR")
                    .Append("ON Img.ImageId = IR.ImageId")
                    .Append("WHERE IR.RecordId = @recordId")
                    .ToString();
                return (await connection.QueryAsync<DiaryImage>(sql, new { recordId })).ToList();
            }
        }

        public async Task<int> GetLinkedRecordsCount(int imageId)
        {
            using (var connection = await _manager.GetConnection())
            {
                return await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM ImagesOfRecord WHERE ImageId = @imageId", new { imageId });
            }
        }
    }
}
