using System.Threading.Tasks;

namespace RiseDiary.Model
{
    public record DeletedEntitiesInfo(int Scopes, int Themes, int Records, int Cogitations, int Images, int RecordThemes, int RecordImages);
    public record SqliteDatabaseInfo(string FileName, string FileSize);

    public interface ISqliteDatabase
    {
        Task<DeletedEntitiesInfo> GetDeletedEntitiesInfo();
        SqliteDatabaseInfo GetSqliteDatabaseInfo();
        Task ClearDatabase();
        Task Vacuum();
        Task File2FileMigration();
    }
}
