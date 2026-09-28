using RiseDiary.Common.Core;
using RiseDiary.Common.Search;
using RiseDiary.WebAPI.Application.Records.Model;

namespace RiseDiary.WebAPI.Application.Search.Abstraction;

public interface IRecordsSearchService
{
    Task<Result<(RecordEntity[] records, int count)>> GetRecords(RecordsFilter filter, CancellationToken cancellationToken = default);
    Task<Result<RecordEntity[]>> GetThisDayRecords(int month, int day, CancellationToken cancellationToken = default);
    Task<Result<RecordEntity[]>> GetRecordsByIds(IEnumerable<Guid> recordsIds, CancellationToken cancellationToken = default);
}
