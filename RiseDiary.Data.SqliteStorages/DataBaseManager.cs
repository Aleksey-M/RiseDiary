using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RiseDiary.Data.SqliteStorages.IntegratedTests")]

namespace RiseDiary.Data.SqliteStorages
{
    public class DataBaseManager : IDataBaseManager
    {
        internal DataBaseManager(string pathToDbFile, string dbFileName = "Diary.db")
        {
            DbFilePath = pathToDbFile;
            DbFileName = dbFileName;
        }
        public string DbFileName { get; }
        public string DbFilePath { get; }
        public string DbFileNameFull => Path.Combine(DbFilePath, DbFileName);
        private string DbConnectionString => string.Format(@"Data Source={0}; ", DbFileNameFull);
        private const string sqlCreateRecordsTable =
            "CREATE TABLE Records (" +
            "[RecordId] INTEGER PRIMARY KEY AUTOINCREMENT, " +
            "[RecordDate] DATETIME, " +
            "[RecordCreateDate] DATETIME, " +
            "[RecordModifyDate] DATETIME, " +
            "[RecordName] NVARCHAR(200)," +
            "[RecordText] TEXT);";
        private const string sqlCreateAreasTable = 
            "CREATE TABLE Areas ("+
            "[AreaId] INTEGER PRIMARY KEY AUTOINCREMENT, " +
            "[AreaName] NVARCHAR(200));";
        private const string sqlCreateRecordTypesTable =
            "CREATE TABLE RecordTypes (" +
            "[RecordTypeId] INTEGER PRIMARY KEY AUTOINCREMENT, " +
            "[AreaId] INTEGER NOT NULL, "+
            "[RecordTypeName] NVARCHAR(200));";
        private const string sqlCreateImagesTable =
            "CREATE TABLE Images (" +
            "[ImageId] INTEGER PRIMARY KEY AUTOINCREMENT, " +
            "[ImageName] NVARCHAR(200), " +
            "[CreateDate] DATETIME, " +
            "[Data] BLOB);";
        private const string sqlCreateRecordTypeToRecordTable =
            "CREATE TABLE TypesOfRecord (" +
            "[TypeId] INTEGER, " +
            "[RecordId] INTEGER, PRIMARY KEY (TypeId, RecordId));";
        private const string sqlCreateImageToRecordTable =
            "CREATE TABLE ImagesOfRecord (" +
            "[ImageId] INTEGER, " +
            "[RecordId] INTEGER, PRIMARY KEY (ImageId, RecordId));";
        private const string sqlCreateCogitationsTable =
            "CREATE TABLE Cogitations (" +
            "[CogitationId] INTEGER PRIMARY KEY AUTOINCREMENT," +
            "[RecordId] INTEGER," +
            "[CogitationDate] DATETIME," +
            "[Cogitation] TEXT);";

        public async Task<SqliteConnection> GetConnection()
        {
            SqliteConnection connection = new SqliteConnection();
            if (File.Exists(DbFileNameFull))
            {
                connection.ConnectionString = DbConnectionString;
                await connection.OpenAsync();
                return connection;
            }

            //SqliteConnection.CreateFile(DbFileNameFull);
            connection.ConnectionString = DbConnectionString;
            await connection.OpenAsync();
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = sqlCreateRecordsTable;
                await command.ExecuteNonQueryAsync();
                command.CommandText = sqlCreateRecordTypesTable;
                await command.ExecuteNonQueryAsync();
                command.CommandText = sqlCreateImagesTable;
                await command.ExecuteNonQueryAsync();
                command.CommandText = sqlCreateRecordTypeToRecordTable;
                await command.ExecuteNonQueryAsync();
                command.CommandText = sqlCreateImageToRecordTable;
                await command.ExecuteNonQueryAsync();
                command.CommandText = sqlCreateAreasTable;
                await command.ExecuteNonQueryAsync();
                command.CommandText = sqlCreateCogitationsTable;
                await command.ExecuteNonQueryAsync();
            }

            return connection;
        }
    }
}
