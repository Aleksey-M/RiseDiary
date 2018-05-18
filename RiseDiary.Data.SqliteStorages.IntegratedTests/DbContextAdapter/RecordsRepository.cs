using RiseDiary.Data.SqliteStorages.IntegratedTests.Repositories;
using RiseDiary.Data.SqliteStorages.IntegratedTests.TestDomain;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests.DbContextAdapter
{
    public class RecordsRepository : IRecordsRepository
    {
        private readonly DiaryDbContext _context;
        public RecordsRepository(DiaryDbContext context)
        {
            _context = context;
        }

        public Task<int> AddRecord(DiaryRecord record)
        {
            var rec2 = new RiseDiary.Model.DiaryRecord
            {
                CreateDate = record.RecordCreateDate,
                Date = record.RecordDate,
                Id = record.RecordId,
                ModifyDate = record.RecordModifyDate,
                Name = record.RecordName,
                Text = record.RecordText
            };
            return _context.AddDiaryRecord(rec2);
        }

        public Task DeleteRecord(int id)
        {
            return _context.DeleteRecord(id);
        }

        public async Task<DiaryRecord> FetchRecordById(int id)
        {
            var rec = await _context.FetchRecordById(id);
            return rec == null ? null :
                new DiaryRecord
                {
                    RecordId = rec.Id,
                    RecordCreateDate = rec.CreateDate,
                    RecordDate = rec.Date,
                    RecordModifyDate = rec.ModifyDate,
                    RecordName = rec.Name,
                    RecordText = rec.Text
                };
        }

        public async Task<List<DiaryRecord>> FetchRecordsByMonth(int year, int? month = null)
        {
            var recList = await _context.FetchRecordsByMonth(year, month);
            return recList.Select(rec => new DiaryRecord
            {
                RecordId = rec.Id,
                RecordCreateDate = rec.CreateDate,
                RecordDate = rec.Date,
                RecordModifyDate = rec.ModifyDate,
                RecordName = rec.Name,
                RecordText = rec.Text
            }).ToList();
        }

        public async Task<List<DiaryRecord>> FetchRecordsListFiltered(RecordsFilter filter)
        {
            var recList = await _context.FetchRecordsListFiltered(filter);
            return recList.Select(rec => new DiaryRecord
            {
                RecordId = rec.Id,
                RecordCreateDate = rec.CreateDate,
                RecordDate = rec.Date,
                RecordModifyDate = rec.ModifyDate,
                RecordName = rec.Name,
                RecordText = rec.Text
            }).ToList();
        }
               
        public Task<List<int>> FetchYearsList()
        {
            return _context.FetchYearsList();
        }

        public Task<int> GetFilteredRecordsCount(RecordsFilter filter)
        {
            return _context.GetFilteredRecordsCount(filter);
        }

        public Task<int> GetMonthRecordsCount(int year, int? month = null)
        {
            return _context.GetMonthRecordsCount(year, month);
        }

        public async Task<DiaryRecord> GetRecordByCogitation(int cogitationId)
        {
            var rec = await _context.GetRecordByCogitation(cogitationId);
            return rec == null ? null :
                new DiaryRecord
                {
                    RecordId = rec.Id,
                    RecordCreateDate = rec.CreateDate,
                    RecordDate = rec.Date,
                    RecordModifyDate = rec.ModifyDate,
                    RecordName = rec.Name,
                    RecordText = rec.Text
                };
        }

        public Task<int> UpdateRecord(DiaryRecord record)
        {
            return _context.UpdateRecord(record == null ? null : new Model.DiaryRecord
            {
                Id = record.RecordId,
                CreateDate = record.RecordCreateDate,
                Date = record.RecordDate,
                ModifyDate = record.RecordModifyDate,
                Name = record.RecordName,
                Text = record.RecordText
            });
        }
    }
}
