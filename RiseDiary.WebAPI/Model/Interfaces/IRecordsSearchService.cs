using RiseDiary.Common.Search;

namespace RiseDiary.Model;

public interface IRecordsSearchService
{
    Task<List<RecordEntity>> GetRecordsList(RecordsFilter filter, CancellationToken cancellationToken = default);
    Task<int> GetRecordsCount(RecordsFilter filter, CancellationToken cancellationToken = default);
    Task<List<RecordEntity>> GetThisDayRecords(int month, int day, CancellationToken cancellationToken = default);
    Task<List<RecordEntity>> GetRecordsByIds(IEnumerable<Guid> recordsIds, CancellationToken cancellationToken = default);
}
