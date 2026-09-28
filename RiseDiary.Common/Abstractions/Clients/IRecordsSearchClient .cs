using RiseDiary.Common.Core;
using RiseDiary.Common.Records;
using RiseDiary.Common.Search;

namespace RiseDiary.Common.Abstractions.Clients;

public interface IRecordsSearchClient
{
    Task<Result<RecordsPageDto<RecordListItemDto>>> GetRecords(RecordsSearchFilters filter, int page, CancellationToken ct = default);
    Task<Result<RecordsPageDto<RecordDto>>> GetRecordsExpanded(RecordsSearchFilters filter, int page, CancellationToken ct = default);
    Task<Result<RecordsPageDto<RecordListItemDto>>> SearchRecords(RecordsTextFilter filter, CancellationToken ct = default);
    Task<Result<RecordListItemDto[]>> GetThisDayRecords(DateOnly? date, CancellationToken ct = default);
    Task<Result<List<RecordDto>>> GetBookmarks(CancellationToken ct = default);
}
