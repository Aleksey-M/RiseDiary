namespace RiseDiary.Model;

public interface IRecordsThemesService
{
    Task AddRecordTheme(Guid recordId, IEnumerable<Guid> themesIds);

    Task RemoveRecordTheme(Guid recordId, IEnumerable<Guid> themesIds);

    Task<List<DiaryRecordTheme>> GetRecordThemes(Guid recordId, CancellationToken cancellationToken = default);
}
