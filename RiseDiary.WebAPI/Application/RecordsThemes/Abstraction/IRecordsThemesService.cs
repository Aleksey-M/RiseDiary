namespace RiseDiary.WebAPI.Application.RecordsThemes.Abstraction;

public interface IRecordsThemesService
{
    Task UpdateRecordThemes(Guid recordId, IEnumerable<Guid> themesIds);
}
