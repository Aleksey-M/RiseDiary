namespace RiseDiary.Data.SqliteStorages.IntegratedTests.Repositories
{
    public interface IRepositoriesFactory
    {
        IAreasRepository AreasRepository { get; }
        IRecordTypesRepository RecordTypesRepository { get; }
        IRecordsRepository RecordsRepository { get; }
        ICogitationRepository CogitationRepository { get; }
        IDiaryImagesRepository DiaryImagesRepository { get; }
    }
}
