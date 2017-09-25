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
    public class RecordTypesRepository : IRecordTypesRepository
    {
        private readonly IDataBaseManager _manager;
        internal RecordTypesRepository(IDataBaseManager dbManager)
        {
            _manager = dbManager;
        }
        public async Task<int> AddRecordType(int areaId, string recordTypeName)
        {
            if (string.IsNullOrWhiteSpace(recordTypeName))
            {
                throw new ArgumentException($"Parameter {nameof(recordTypeName)} should not be null or empty");
            }
            using (var connection = await _manager.GetConnection())
            {
                return (await connection.QueryAsync<int>(
                    @"INSERT INTO RecordTypes ( AreaId, RecordTypeName ) VALUES ( @areaId, @recordTypeName ); SELECT last_insert_rowid()",
                    new { areaId, recordTypeName })).First();
            }
        }
        public async Task<int> UpdateRecordType(DiaryRecordType recordType)
        {
            using (var connection = await _manager.GetConnection())
            {
                await connection.ExecuteAsync(
                    @"UPDATE RecordTypes SET RecordTypeName=@RecordTypeName WHERE RecordTypeId=@RecordTypeId; SELECT last_insert_rowid()",
                    recordType);
                return recordType.RecordTypeId;
            }
        }
        public async Task DeleteRecordType(int recordTypeId)
        {
            using (var connection = await _manager.GetConnection())
            {
                await connection.ExecuteAsync(@"DELETE FROM TypesOfRecord WHERE TypeId=@recordTypeId;", new { recordTypeId });
                await connection.ExecuteAsync(@"DELETE FROM RecordTypes WHERE RecordTypeId=@recordTypeId;", new { recordTypeId });
            }
        }
        public async Task<DiaryRecordType> FetchRecordTypeById(int recordTypeId)
        {
            using (var connection = await _manager.GetConnection())
            {
                return (await connection.QueryAsync<DiaryRecordType>(@"SELECT * FROM RecordTypes WHERE RecordTypeId = @recordTypeId", new { recordTypeId })).FirstOrDefault();
            }
        } 
        public async Task<List<DiaryRecordType>> FetchRecordTypesOfArea(int? areaId)
        {
            using (var connection = await _manager.GetConnection())
            {
                if(areaId != null)
                {
                    return (await connection.QueryAsync<DiaryRecordType>(@"SELECT * FROM RecordTypes WHERE AreaId=@areaId", new { areaId })).ToList();
                }
                return (await connection.QueryAsync<DiaryRecordType>(@"SELECT * FROM RecordTypes")).ToList();
            }
        }
        public async Task<List<int>> FetchRecordTypesIds(int? areaId)
        {
            using (var connection = await _manager.GetConnection())
            {
                if (areaId != null)
                {
                    return (await connection.QueryAsync<int>(@"SELECT RecordTypeId FROM RecordTypes WHERE AreaId = @areaId", new { areaId })).ToList();
                }
                return (await connection.QueryAsync<int>(@"SELECT RecordTypeId FROM RecordTypes")).ToList();
            }
        }
        public async Task<List<DiaryRecordTypeJoined>> FetchRecordTypesWithAreas()
        {
            using (var connection = await _manager.GetConnection())
            {
                return (await connection.QueryAsync<DiaryRecordTypeJoined>(@"SELECT * FROM RecordTypes AS rt LEFT OUTER JOIN Areas AS ar ON rt.AreaId=ar.AreaId")).ToList();
            }
        }
        public async Task<int> GetRecordTypesCount(int? areaId)
        {
            using (var connection = await _manager.GetConnection())
            {
                if(areaId != null)
                {
                    return (await connection.QueryAsync<int>(@"SELECT COUNT(*) FROM RecordTypes WHERE AreaId=@areaId", new { areaId })).First();
                }
                return (await connection.QueryAsync<int>(@"SELECT COUNT(*) FROM RecordTypes")).First();
            }
        }

        public async Task AddTypeForRecord(int recordId, int typeId)
        {
            using (var connection = await _manager.GetConnection())
            {
                await connection.ExecuteAsync("INSERT INTO TypesOfRecord VALUES (@typeId, @recordId)", new { recordId, typeId});
            }
        }

        public async Task RemoveTypeForRecord(int recordId, int typeId)
        {
            using (var connection = await _manager.GetConnection())
            {
                await connection.ExecuteAsync("DELETE FROM TypesOfRecord WHERE TypeId = @typeId AND RecordId = @recordId", new { recordId, typeId });
            }
        }

        public async Task<List<DiaryRecordType>> FetchTypesForRecord(int recordId)
        {
            using (var connection = await _manager.GetConnection())
            {
                var sql = new StringBuilder(
                    "SELECT R.RecordTypeId, R.AreaId, R.RecordTypeName FROM RecordTypes AS R")
                    .Append("INNER JOIN TypesOfRecord AS TR")
                    .Append("ON R.RecordTypeId = TR.TypeId")
                    .Append("WHERE TR.RecordId = @recordId")
                    .ToString();
                return (await connection.QueryAsync<DiaryRecordType>(sql, new { recordId })).ToList();               
            }
        }
    }
}