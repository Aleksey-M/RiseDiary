using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace RiseDiary.Data.SqliteStorages
{
    public interface IDataBaseManager
    {
        string DbFileName { get; }
        string DbFileNameFull { get; }
        string DbFilePath { get; }

        Task<SqliteConnection> GetConnection();
    }
}