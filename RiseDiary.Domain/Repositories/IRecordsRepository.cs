using RiseDiary.Domain.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.Domain.Repositories
{
    public sealed class RecordsFilter
    {
        public string RecordNameFilter { get; set; }

        private DateTime? _recordDateFrom;
        private DateTime? _recordDateTo;
                
        public DateTime? RecordDateFrom
        {
            get => _recordDateFrom;
            set => _recordDateFrom = value > _recordDateTo ? null : value?.Date;
        }
        public DateTime? RecordDateTo
        {
            get => _recordDateTo;
            set => _recordDateTo = value < _recordDateFrom ? null : value?.Date;
        }
        public int PageNo { get; set; } = 0;
        public int PageSize { get; set; } = 20;
        public static RecordsFilter Empty => new RecordsFilter();
        public static bool IsEmpty(RecordsFilter filter) =>
            string.IsNullOrWhiteSpace(filter.RecordNameFilter) &&
            filter.RecordDateFrom == null &&
            filter.RecordDateTo == null &&
            filter.RecordTypeIds.Count == 0;

        private ReadOnlyCollection<int> _recordTypesIds = new ReadOnlyCollection<int>(new List<int>());
        public ReadOnlyCollection<int> RecordTypeIds { get => _recordTypesIds; }
        public void AddRecordTypeId(int rtid)
        {
            if (!_recordTypesIds.Contains(rtid))
            {
                var list = new List<int>(_recordTypesIds)
                {
                    rtid
                };
                _recordTypesIds = new ReadOnlyCollection<int>(list); 
            }
        }
        public void AddRecordTypeId(IEnumerable<int> idsList)
        {
            if (idsList.Any(i => !_recordTypesIds.Contains(i)))
            {                
                _recordTypesIds = new ReadOnlyCollection<int>(_recordTypesIds.Union(idsList).ToList());
            }
        }
        public void RemoveRecordTypeId(int id)
        {
            if (_recordTypesIds.Contains(id))
            {
                _recordTypesIds = new ReadOnlyCollection<int>(_recordTypesIds.Where(i => i != id).ToList());
            }
        }
        public void RemoveRecordTypeId(IEnumerable<int> idsList)
        {
            var foundIds = _recordTypesIds.Intersect(idsList);
            if (foundIds.Count() > 0)
            {
                _recordTypesIds = new ReadOnlyCollection<int>(_recordTypesIds.Except(idsList).ToList());
            }
        }
        public bool IsEmptyTypeFilter => _recordTypesIds.Count == 0;
    }

    public interface IRecordsRepository
    {
        Task<int> AddRecord(DiaryRecord record);
        Task<DiaryRecord> FetchRecordById(int id);
        Task DeleteRecord(int id);
        Task<int> UpdateRecord(DiaryRecord record);
        Task<int> GetFilteredRecordsCount(RecordsFilter filter);
        Task<List<DiaryRecord>> FetchRecordsListFiltered(RecordsFilter filter);
        Task<List<DiaryRecord>> FetchRecordsByMonth(int year, int? month = null);
        Task<int> GetMonthRecordsCount(int year, int? month=null);
        Task<DiaryRecord> GetRecordByCogitation(int cogitationId);
    }
}
