using Dapper;
using RiseDiary.Domain.Model;
using RiseDiary.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.Data.SqliteStorages
{
    public class AreasRepository : IAreasRepository
    {
        private readonly IDataBaseManager _manager;
        public AreasRepository(IDataBaseManager dbManager)
        {
            _manager = dbManager;
        }
        public async Task<int> AddArea(string areaName)
        {
            if (string.IsNullOrWhiteSpace(areaName))
            {
                throw new ArgumentException($"Parameter {nameof(areaName)} should not be null or empty");
            }
            using (var connection = await _manager.GetConnection())
            {
                return (await connection.QueryAsync<int>(
                    @"INSERT INTO Areas ( AreaName ) VALUES ( @areaName ); SELECT last_insert_rowid()", 
                    new { areaName })).First();                
            }
        }
        public async Task<bool> CanDeleteArea(int areaId)
        {
            using (var connection = await _manager.GetConnection())
            {
                return await connection.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM RecordTypes WHERE AreaId=@areaId;", new { areaId }) == 0;
            }
        }
        public async Task DeleteArea(int areaId)
        {
            using (var connection = await _manager.GetConnection())
            {
                await connection.ExecuteAsync(@"DELETE FROM Areas WHERE AreaId=@areaId;", new { areaId });
            }
        }
        public async Task<int> UpdateArea(DiaryArea area)
        {
            using (var connection = await _manager.GetConnection())
            {
                int updated = await connection.ExecuteAsync(@"UPDATE Areas SET AreaName=@AreaName WHERE AreaId=@AreaId", area);
                if(updated == 0)
                {
                    throw new ArgumentException($"Area with id = {area.AreaId} is not exists");
                }
                return area.AreaId;
            }
        }
        public async Task<DiaryArea> FetchAreaById(int areaId)
        {
            using (var connection = await _manager.GetConnection())
            {
                return (await connection.QueryAsync<DiaryArea>(@"SELECT * FROM Areas WHERE AreaId = @areaId", new { areaId })).FirstOrDefault();
            }
        }
        public async Task<List<DiaryArea>> FetchAllAreas()
        {
            using (var connection = await _manager.GetConnection())
            {
                return (await connection.QueryAsync<DiaryArea>(@"SELECT * FROM Areas")).ToList();
            }
        }
        public async Task<int> GetAreasCount()
        {
            using (var connection = await _manager.GetConnection())
            {
                return (await connection.QueryAsync<int>(@"SELECT COUNT(*) FROM Areas")).First();
            }
        }
    }
}
