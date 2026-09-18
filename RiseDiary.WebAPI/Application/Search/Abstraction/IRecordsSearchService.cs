using RiseDiary.Common.Search;
using RiseDiary.WebAPI.Application.Records.Model;

namespace RiseDiary.WebAPI.Application.Search.Abstraction;

public interface IRecordsSearchService
{
    Task<List<RecordEntity>> GetRecordsList(RecordsFilter filter, CancellationToken cancellationToken = default);
    Task<int> GetRecordsCount(RecordsFilter filter, CancellationToken cancellationToken = default);
    Task<List<RecordEntity>> GetThisDayRecords(int month, int day, CancellationToken cancellationToken = default);
    Task<List<RecordEntity>> GetRecordsByIds(IEnumerable<Guid> recordsIds, CancellationToken cancellationToken = default);
}
