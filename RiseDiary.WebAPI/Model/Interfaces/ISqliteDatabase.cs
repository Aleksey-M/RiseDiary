using RiseDiary.Shared.Database;

namespace RiseDiary.Model;

public interface ISqliteDatabase
{
    Task<DeletedEntitiesCount> GetDeletedEntitiesCount(CancellationToken cancellationToken = default);

    SqliteDatabaseFileInfo GetSqliteDatabaseInfo();

    Task ClearDatabase();

    Task Vacuum();

    Task<DeletedDataInfo> GetDeletedEntitiesData(CancellationToken cancellationToken = default);
}
