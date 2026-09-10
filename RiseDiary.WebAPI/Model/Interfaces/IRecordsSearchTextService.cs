using RiseDiary.Common.Search;

namespace RiseDiary.Model;

public interface IRecordsSearchTextService
{
    Task<List<RecordEntity>> GetRecordsList(RecordsTextFilter filter, CancellationToken cancellationToken = default);
    Task<int> GetRecordsCount(string? searchText, CancellationToken cancellationToken = default);
}
