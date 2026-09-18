using RiseDiary.Common.Search;
using RiseDiary.WebAPI.Application.Records.Model;

namespace RiseDiary.WebAPI.Application.Search.Abstraction;

public interface IRecordsSearchTextService
{
    Task<List<RecordEntity>> GetRecordsList(RecordsTextFilter filter, CancellationToken cancellationToken = default);
    Task<int> GetRecordsCount(string? searchText, CancellationToken cancellationToken = default);
}
