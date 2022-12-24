namespace RiseDiary.Model;

public interface IRecordsThemesService
{
    Task UpdateRecordThemes(Guid recordId, IEnumerable<Guid> themesIds);
}
