using RiseDiary.Common.Database;

namespace RiseDiary.WebAPI.Data;

public interface ISqliteDatabase
{
    Task<DeletedEntitiesCount> GetDeletedEntitiesCount(CancellationToken cancellationToken = default);
    SqliteDatabaseFileInfo GetSqliteDatabaseInfo();
    Task ClearDatabase();
    Task Vacuum();
    Task<DeletedDataInfo> GetDeletedEntitiesData(CancellationToken cancellationToken = default);
}
