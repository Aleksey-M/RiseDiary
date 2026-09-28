using RiseDiary.Common.Core;
using RiseDiary.Common.Search;
using RiseDiary.WebAPI.Application.Records.Model;

namespace RiseDiary.WebAPI.Application.Search.Abstraction;

public interface IRecordsSearchTextService
{
    Task<Result<(RecordEntity[] records, int count)>> GetRecordsList(RecordsTextFilter filter, CancellationToken cancellationToken = default);
}
