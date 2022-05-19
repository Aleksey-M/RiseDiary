using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Model
{
    public record DeletedEntitiesCount(int Scopes, int Themes, int Records, int Cogitations, int Images, int RecordThemes, int RecordImages);
    
    public record SqliteDatabaseFileInfo(string FileName, string FileSize);

    //public record DeletedRecord(Guid Id, DateOnly Date, string Name, string Text, 
    //    List<(Guid, DateTime, string)> Cogitations, List<string> Themes, List<(Guid, string)> Images);

    public interface ISqliteDatabase
    {
        Task<DeletedEntitiesCount> GetDeletedEntitiesCount();

        SqliteDatabaseFileInfo GetSqliteDatabaseInfo();

        Task ClearDatabase();

        Task Vacuum();

        Task File2FileMigration();
    }
}
