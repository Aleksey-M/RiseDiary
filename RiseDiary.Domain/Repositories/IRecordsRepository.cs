using RiseDiary.Domain.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Domain.Repositories
{
    public sealed class RecordsFilter
    {
        public string RecordNameFilter { get; set; }
        public DateTime? RecordDateFrom { get; set; }
        public DateTime? RecordDateTo { get; set; }
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public RecordsFilter Empty => new RecordsFilter();

        public List<int> AreaIds { get; set; } = new List<int>();
        public List<int> RecordTypeIds { get; set; } = new List<int>();
    }

    public interface IRecordsRepository
    {
        Task<int> AddRecord(DiaryRecord record);
        Task<DiaryRecord> FetchRecordById(int id);
        Task DeleteRecord(int id);
        Task<int> UpdateRecord(DiaryRecord record);
        Task<int> GetFilteredRecordsCount(RecordsFilter filter);
        Task<List<DiaryRecord>> FetchRecordsListFiltered(RecordsFilter filter);
        Task<List<DiaryRecord>> FetchRecordsByMonth(uint year, int? month = null);
        Task<int> GetMonthRecordsCount(uint year, int? month=null);
        Task<DiaryRecord> GetRecordByCogitation(int cogitationId);
    }
}
