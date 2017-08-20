using System;

namespace RiseDiary.Data.SqliteStorages
{
    internal sealed class RecordsFilter
    {
        public string RecordNameFilter { get; set; }
        public DateTime? RecordDateFrom { get; set; }
        public DateTime? RecordDateTo { get; set; }
        public RecordsFilter Empty => new RecordsFilter();
    } 
    /*
    internal interface IRecordsStorage
    {
        void SaveRecord(DiaryRecord record);
        DiaryRecord GetRecord(int id);
        void DeleteRecord(int id);
        void UpdateRecord(DiaryRecord record);

        int GetRecordsCount(RecordsFilter filter);
        IEnumerable<DiaryRecord> GetRecords(RecordsFilter filter);
    }
    */
    public class RecordsStorage
    {
        private readonly DataBaseManager _manager;
        public RecordsStorage(DataBaseManager manager)
        {
            _manager = manager;
        }

    }
}
