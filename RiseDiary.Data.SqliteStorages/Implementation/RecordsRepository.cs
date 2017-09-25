using System.Collections.Generic;
using System.Threading.Tasks;
using RiseDiary.Domain.Model;
using RiseDiary.Domain.Repositories;
using System;
using Dapper;
using System.Linq;
using System.Text;

namespace RiseDiary.Data.SqliteStorages
{   
    public class RecordsRepository : IRecordsRepository
    {
        private readonly DataBaseManager _manager;
        internal RecordsRepository(DataBaseManager manager)
        {
            _manager = manager;
        }

        public async Task DeleteRecord(int id)
        {
            using (var connection = await _manager.GetConnection())
            {
                await connection.ExecuteAsync(@"DELETE FROM Cogitations WHERE RecordId = @id;", new { id });
                await connection.ExecuteAsync(@"DELETE FROM TypesOfRecord WHERE RecordId = @id;", new { id });
                await connection.ExecuteAsync(@"DELETE FROM ImagesOfRecord WHERE RecordId = @id;", new { id });
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

        public async Task<DiaryRecord> GetRecordByCogitation(int cogitationId)
        {
            using (var connection = await _manager.GetConnection())
            {
                int? recId = (await connection.QueryAsync<int>("SELECT RecordId FROM Cogitations WHERE CogitationId = @cogitationId", new { cogitationId })).FirstOrDefault();
                if (recId != null)
                {
                    return (await connection.QueryAsync<DiaryRecord>("SELECT * FROM Records WHERE RecordId = @recId", new { recId = (int)recId })).FirstOrDefault();
                }
                else return null;
            }
        }

        private string GenerateWhereSqlPart(RecordsFilter filter)
        {
            if (RecordsFilter.IsEmpty(filter)) return "";

            var whereClause = new StringBuilder(string.IsNullOrWhiteSpace(filter.RecordNameFilter) ? " WHERE 1" : $" WHERE RecordName LIKE \"%{filter.RecordNameFilter}%\"");

            if(filter.RecordDateFrom != null)
            {
                whereClause.Append(" AND RecordDate >= \"").Append(filter.RecordDateFrom?.ToString("o")).Append("\"");
            }
            if (filter.RecordDateTo != null)
            {
                whereClause.Append(" AND RecordDate <= \"").Append(filter.RecordDateTo?.ToString("o")).Append("\"");
            }
            return whereClause.ToString();
        }

        private string GenerateInSqlPart(IEnumerable<int> Ids)
        {
            var sb = new StringBuilder("(").Append(Ids.First().ToString());
            for(int i = 1; i < Ids.Count(); i++)
            {
                sb.Append(", ").Append(Ids.ElementAt(i).ToString());
            }
            sb.Append(")");
            return sb.ToString();
        }

        public async Task<List<DiaryRecord>> FetchRecordsListFiltered(RecordsFilter filter)
        {
            using (var connection = await _manager.GetConnection())
            {
                IEnumerable<DiaryRecord> list;
                string wherePart = GenerateWhereSqlPart(filter);
                if (filter.IsEmptyTypeFilter)
                {
                    list = await connection.QueryAsync<DiaryRecord>("SELECT * FROM Records " + wherePart);                    
                }
                else
                {
                    string sql = new StringBuilder(
                        "SELECT DISTINCT R.RecordId, R.RecordDate, R.RecordCreateDate, R.RecordModifyDate, R.RecordName, R.RecordText ")
                        .Append("FROM Records AS R ")
                        .Append("INNER JOIN TypesOfRecord AS T ")
                        .Append("ON R.RecordId = T.RecordId ")
                        .Append(wherePart)
                        .Append(" AND T.TypeId IN ")
                        .Append(GenerateInSqlPart(filter.RecordTypeIds))
                        .ToString();
                    list = await connection.QueryAsync<DiaryRecord>(sql);
                }
                return list.Skip(filter.PageNo * filter.PageSize).Take(filter.PageSize).ToList();
            }
        }

        public async Task<int> GetFilteredRecordsCount(RecordsFilter filter)
        {
            using (var connection = await _manager.GetConnection())
            {
                string wherePart = GenerateWhereSqlPart(filter);
                if (filter.IsEmptyTypeFilter)
                {
                    return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Records " + wherePart);
                }
                else
                {
                    string sql = new StringBuilder(
                        "SELECT COUNT(DISTINCT R.RecordId) ")
                        .Append("FROM Records AS R ")
                        .Append("INNER JOIN TypesOfRecord AS T ")
                        .Append("ON R.RecordId = T.RecordId ")
                        .Append(wherePart)
                        .Append(" AND T.TypeId IN ")
                        .Append(GenerateInSqlPart(filter.RecordTypeIds))
                        .ToString();
                    return await connection.ExecuteScalarAsync<int>(sql);
                }
            }
        }

        public async Task<List<DiaryRecord>> FetchRecordsByMonth(int year, int? month = null)
        {            
            using (var connection = await _manager.GetConnection())
            {                
                List<DiaryRecord> list;
                if (month == null)
                {
                    list = (await connection.QueryAsync<DiaryRecord>(@"SELECT * FROM Records WHERE strftime('%Y', RecordDate) = @y", new { y = year.ToString()})).ToList();
                }
                else
                {
                    list = (await connection.QueryAsync<DiaryRecord>(@"SELECT * FROM Records WHERE strftime('%Y', RecordDate) = @y AND strftime('%m', RecordDate) = @m", new { y = year.ToString(), m = ((int)month).ToString("00") })).ToList();                    
                }
                return list;
            }
        }

        public async Task<int> GetMonthRecordsCount(int year, int? month = null)
        {
            using (var connection = await _manager.GetConnection())
            {
                int recCount;
                if (month == null)
                {
                    recCount = (await connection.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM Records WHERE strftime('%Y', RecordDate) = @y", new { y = year.ToString() }));
                }
                else
                {
                    recCount = (await connection.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM Records WHERE strftime('%Y', RecordDate) = @y AND strftime('%m', RecordDate) = @m", new { y = year.ToString(), m = ((int)month).ToString("00") }));
                }
                return recCount;
            }
        }

        public async Task<int> AddRecord(DiaryRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            using(var connection = await _manager.GetConnection())
            {
                return (await connection.ExecuteScalarAsync<int>(
                    @"INSERT INTO Records ( RecordDate, RecordCreateDate, RecordModifyDate, RecordName, RecordText ) VALUES ( @RecordDate, @RecordCreateDate, @RecordModifyDate, @RecordName, @RecordText ); SELECT last_insert_rowid()",
                    record));
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
