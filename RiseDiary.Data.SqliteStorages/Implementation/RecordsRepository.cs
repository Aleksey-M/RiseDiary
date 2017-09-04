using System.Collections.Generic;
using System.Threading.Tasks;
using RiseDiary.Domain.Model;
using RiseDiary.Domain.Repositories;
using System;
using Dapper;
using System.Linq;

namespace RiseDiary.Data.SqliteStorages
{   
    public class RecordsRepository : IRecordsRepository
    {
        private readonly DataBaseManager _manager;
        public RecordsRepository(DataBaseManager manager)
        {
            _manager = manager;
        }

        public async Task DeleteRecord(int id)
        {
            using (var connection = await _manager.GetConnection())
            {
                await connection.ExecuteAsync(@"DELETE FROM Records WHERE RecordId = @id;", new { id });
            }
        }

        public async Task<DiaryRecord> FetchRecordById(int id)
        {
            using (var connection = await _manager.GetConnection())
            {
                var rec = (await connection.QueryAsync<DiaryRecord>("SELECT * FROM Records WHERE RecordId = @id", new { id })).FirstOrDefault();
                return rec;
            }
        }

        public Task<DiaryRecord> GetRecordByCogitation(int cogitationId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<DiaryRecord>> FetchRecordsListFiltered(RecordsFilter filter)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<DiaryRecord>> FetchRecordsByMonth(uint year, int? month = null)
        {
            throw new NotImplementedException();
            using (var connection = await _manager.GetConnection())
            {

            }
        }

        public Task<int> GetFilteredRecordsCount(RecordsFilter filter)
        {
            throw new System.NotImplementedException();
        }

        public Task<int> GetMonthRecordsCount(uint year, int? month = null)
        {
            throw new System.NotImplementedException();
        }

        public async Task<int> AddRecord(DiaryRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            using(var connection = await _manager.GetConnection())
            {
                return (await connection.QueryAsync<int>(
                    @"INSERT INTO Records ( RecordDate, RecordCreateDate, RecordModifyDate, RecordName, RecordText ) VALUES ( @RecordDate, @RecordCreateDate, @RecordModifyDate, @RecordName, @RecordText ); SELECT last_insert_rowid()",
                    record)).First();
            }
        }

        public async Task<int> UpdateRecord(DiaryRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            using (var connection = await _manager.GetConnection())
            {
                int updCount = await connection.ExecuteAsync(
                    @"UPDATE Records SET RecordDate=@RecordDate, RecordCreateDate=@RecordCreateDate, RecordModifyDate=@RecordModifyDate, RecordName=@RecordName, RecordText=@RecordText WHERE RecordId = @RecordId",
                    record);
                if(updCount != 1)
                {
                    throw new ArgumentException($"Record with ID={record.RecordId} not found");
                }
                return record.RecordId;
            }
        }
    }
}
