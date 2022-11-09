namespace RiseDiary.Model;

public interface IRecordsSearchTextService
{
    Task<List<DiaryRecord>> GetRecordsList(RecordsTextFilter filter, CancellationToken cancellationToken = default);

    Task<int> GetRecordsCount(string? searchText, CancellationToken cancellationToken = default);
}
