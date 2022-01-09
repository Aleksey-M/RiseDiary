using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.Model.Services
{
    public class SqliteDatabase : ISqliteDatabase
    {
        private readonly IConfiguration _config;
        private readonly DiaryDbContext _context;

        public SqliteDatabase(IConfiguration config, DiaryDbContext context)
        {
            _config = config;
            _context = context;
        }

        public async Task ClearDatabase()
        {
            bool oldValue = _context.SoftDeleting;
            _context.SoftDeleting = false;

            var records = _context.Records.IgnoreQueryFilters().Where(r => r.Deleted);
            _context.Records.RemoveRange(records);

            var images = _context.Images.IgnoreQueryFilters().Where(i => i.Deleted);
            _context.Images.RemoveRange(images);

            var scopes = _context.Scopes.IgnoreQueryFilters().Where(s => s.Deleted);
            _context.Scopes.RemoveRange(scopes);

            await _context.SaveChangesAsync().ConfigureAwait(false);

            var themes = _context.Themes.IgnoreQueryFilters().Where(t => t.Deleted);
            _context.Themes.RemoveRange(themes);

            await _context.SaveChangesAsync().ConfigureAwait(false);

            var recImages = _context.RecordImages.IgnoreQueryFilters().Where(ri => ri.Deleted);
            _context.RecordImages.RemoveRange(recImages);

            var recThemes = _context.RecordThemes.IgnoreQueryFilters().Where(rt => rt.Deleted);
            _context.RecordThemes.RemoveRange(recThemes);

            var cogitations = _context.Cogitations.IgnoreQueryFilters().Where(r => r.Deleted);
            _context.Cogitations.RemoveRange(cogitations);

            await _context.SaveChangesAsync().ConfigureAwait(false);

            _context.SoftDeleting = oldValue;
        }

        public async Task<DeletedEntitiesInfo> GetDeletedEntitiesInfo()
        {
            int records = await _context.Records.IgnoreQueryFilters().CountAsync(r => r.Deleted).ConfigureAwait(false);
            int images = await _context.Images.IgnoreQueryFilters().CountAsync(i => i.Deleted).ConfigureAwait(false);
            int scopes = await _context.Scopes.IgnoreQueryFilters().CountAsync(s => s.Deleted).ConfigureAwait(false);
            int themes = await _context.Themes.IgnoreQueryFilters().CountAsync(t => t.Deleted).ConfigureAwait(false);
            int recImages = await _context.RecordImages.IgnoreQueryFilters().CountAsync(ri => ri.Deleted).ConfigureAwait(false);
            int recThemes = await _context.RecordThemes.IgnoreQueryFilters().CountAsync(rt => rt.Deleted).ConfigureAwait(false);
            int cogitations = await _context.Cogitations.IgnoreQueryFilters().CountAsync(r => r.Deleted).ConfigureAwait(false);

            return new DeletedEntitiesInfo(scopes, themes, records, cogitations, images, recThemes, recImages);
        }

        public SqliteDatabaseInfo GetSqliteDatabaseInfo()
        {
            var dataBaseFileName = _config.GetValue<string>("dbFile");
            var dataBaseFileSize = Math.Round(new FileInfo(dataBaseFileName).Length / 1024f / 1024f, 2).ToString(CultureInfo.InvariantCulture) + " Mb";

            return new SqliteDatabaseInfo(dataBaseFileName, dataBaseFileSize);
        }

        public async Task Vacuum() => await _context.Database.ExecuteSqlRawAsync("vacuum;").ConfigureAwait(false);

        public async Task File2FileMigration()
        {
            var sourceFile = _config.GetValue<string>("dbFile");
            var path = Path.GetDirectoryName(sourceFile) ?? throw new Exception("Directory does not exists for file: " + sourceFile);
            var backupFolder = Path.Combine(path, "backup");
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }
            var backupFileName = Path.Combine(backupFolder, "Migration " + DateTime.Now.ToString("yyyy.MM.dd-hh_mm_ss", CultureInfo.InvariantCulture) + ".bak");

            using var context = await CreateCurrentContext(backupFileName).ConfigureAwait(false);
            // old SQLite file
            var connStrSource = $@"Data Source={sourceFile};";
            using var connection = new SqliteConnection(connStrSource);
            connection.Open();

            var scopes = await ReadScopes(connection).ConfigureAwait(false);
            var themes = await ReadThemes(connection).ConfigureAwait(false);
            await WriteScopesAndThemes(context, scopes, themes).ConfigureAwait(false);

            var images = await ReadImages(connection).ConfigureAwait(false);
            var fullImages = await ReadFullImages(connection).ConfigureAwait(false);
            await WriteImages(context, images, fullImages).ConfigureAwait(false);

            var settings = await ReadAppSettings(connection).ConfigureAwait(false);
            await WriteAppSettings(context, settings).ConfigureAwait(false);

            var records = await ReadRecords(connection).ConfigureAwait(false);
            await WriteRecords(context, records).ConfigureAwait(false);

            var cogitations = await ReadCogitations(connection).ConfigureAwait(false);
            await WriteCogitations(context, cogitations).ConfigureAwait(false);

            var recThemes = await ReadRecordThemes(connection).ConfigureAwait(false);
            await WriteRecordsThemes(context, recThemes).ConfigureAwait(false);

            var recImages = await ReadRecordsImages(connection).ConfigureAwait(false);
            await WriteRecordsImages(context, recImages).ConfigureAwait(false);
        }

        private static async Task<DiaryDbContext> CreateCurrentContext(string fileName)
        {
            var connStrMigrated = $@"Data Source={fileName};";

            IConfigurationRoot configuration = new ConfigurationBuilder().Build();
            var builder = new DbContextOptionsBuilder<DiaryDbContext>();
            builder.UseSqlite(connStrMigrated);
            var context = new DiaryDbContext(builder.Options);
            await context.Database.MigrateAsync().ConfigureAwait(false);

            return context;
        }

        private static async Task<List<(string? Id, string? Name, bool Deleted)>> ReadScopes(SqliteConnection connection)
        {
            await CheckColumnsCount(connection, "Scopes", 3).ConfigureAwait(false);
            //
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Scopes";

            var scopes = new List<(string? Id, string? Name, bool Deleted)>();
            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                scopes.Add((
                    reader["Id"].ToString(),
                    reader["ScopeName"].ToString(),
                    Convert.ToBoolean(reader["Deleted"], CultureInfo.InvariantCulture)));
            }
            await reader.CloseAsync().ConfigureAwait(false);

            return scopes;
        }

        private static async Task<List<(string? Id, string? ScopeId, string? ThemeName, bool Deleted, bool Actual)>> ReadThemes(SqliteConnection connection)
        {
            await CheckColumnsCount(connection, "Themes", 5).ConfigureAwait(false);
            //
            var themes = new List<(string? Id, string? ScopeId, string? ThemeName, bool Deleted, bool Actual)>();
            using var c2 = connection.CreateCommand();
            c2.CommandText = "SELECT * FROM Themes";

            using var r2 = await c2.ExecuteReaderAsync().ConfigureAwait(false);
            while (await r2.ReadAsync().ConfigureAwait(false))
            {
                themes.Add((
                    r2["Id"].ToString(),
                    r2["ScopeId"].ToString(),
                    r2["ThemeName"].ToString(),
                    Convert.ToBoolean(r2["Deleted"], CultureInfo.InvariantCulture),
                    Convert.ToBoolean(r2["Actual"], CultureInfo.InvariantCulture)));
            }
            await r2.CloseAsync().ConfigureAwait(false);

            return themes;
        }

        private static async Task WriteScopesAndThemes(DiaryDbContext context, List<(string? Id, string? Name, bool Deleted)> scopes, List<(string? Id, string? ScopeId, string? ThemeName, bool Deleted, bool Actual)> themes)
        {
            var sList = new List<DiaryScope>();
            foreach (var (Id, Name, Deleted) in scopes)
            {
                var sId = Guid.Parse(Id ?? throw new ArgumentException($"Id for scope '{Name}' is null"));

                var s = new DiaryScope
                {
                    Id = sId,
                    ScopeName = Name ?? "",
                    Deleted = Deleted,
                    Themes = new List<DiaryTheme>()
                };

                foreach (var theme in themes.Where(t => t.ScopeId == Id))
                {
                    var tId = Guid.Parse(theme.Id ?? throw new ArgumentException($"Id for theme '{theme.ThemeName}' is null"));
                    var t = new DiaryTheme
                    {
                        Id = tId,
                        ScopeId = s.Id,
                        ThemeName = theme.ThemeName!,
                        Deleted = theme.Deleted,
                        Actual = theme.Actual
                    };

                    s.Themes.Add(t);
                }

                sList.Add(s);
            }

            await context.Scopes.AddRangeAsync(sList).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        private static async Task<List<(string? Id, string? Name, string? CreateDate, string? ModifyDate, byte[] Thumbnail, int Width, int Height, int SizeByte, bool Deleted, DateTime? Taken, string? cameraModel)>> ReadImages(SqliteConnection connection)
        {
            await CheckColumnsCount(connection, "Images", 11).ConfigureAwait(false);
            //
            var images = new List<(string? Id, string? Name, string? CreateDate, string? ModifyDate, byte[] Thumbnail, int Width, int Height, int SizeByte, bool Deleted, DateTime? Taken, string? cameraModel)>();
            using var c3 = connection.CreateCommand();
            c3.CommandText = "SELECT * FROM Images";
            using var r3 = await c3.ExecuteReaderAsync().ConfigureAwait(false);
            while (await r3.ReadAsync().ConfigureAwait(false))
            {
                images.Add((
                    r3["Id"].ToString(),
                    r3["Name"].ToString(),
                    r3["CreateDate"].ToString(),
                    r3["ModifyDate"].ToString(),
                    (byte[])r3["Thumbnail"],
                    Convert.ToInt32(r3["Width"], CultureInfo.InvariantCulture),
                    Convert.ToInt32(r3["Height"], CultureInfo.InvariantCulture),
                    Convert.ToInt32(r3["SizeByte"], CultureInfo.InvariantCulture),
                    Convert.ToBoolean(r3["Deleted"], CultureInfo.InvariantCulture),
                    string.IsNullOrEmpty(r3["Taken"].ToString()) ? default : Convert.ToDateTime(r3["Taken"].ToString(), CultureInfo.InvariantCulture),
                    r3["CameraModel"].ToString()));
            }
            await r3.CloseAsync().ConfigureAwait(false);

            return images;
        }

        private static async Task<List<(string? Id, string? ImageId, byte[] Data)>> ReadFullImages(SqliteConnection connection)
        {
            await CheckColumnsCount(connection, "FullSizeImages", 3).ConfigureAwait(false);
            //
            var fullImages = new List<(string? Id, string? ImageId, byte[] Data)>();
            using var c4 = connection.CreateCommand();
            c4.CommandText = "SELECT * FROM FullSizeImages";
            using var r4 = await c4.ExecuteReaderAsync().ConfigureAwait(false);
            while (await r4.ReadAsync().ConfigureAwait(false))
            {
                fullImages.Add((
                    r4["Id"].ToString(),
                    r4["ImageId"].ToString(),
                    (byte[])r4["Data"]));
            }
            await r4.CloseAsync().ConfigureAwait(false);

            return fullImages;
        }

        private static async Task WriteImages(DiaryDbContext context, List<(string? Id, string? Name, string? CreateDate, string? ModifyDate, byte[] Thumbnail, int Width, int Height, int SizeByte, bool Deleted, DateTime? Taken, string? cameraModel)> images, List<(string? Id, string? ImageId, byte[] Data)> fullImages)
        {
            var imgList = new List<DiaryImage>();
            foreach (var (Id, Name, CreateDate, ModifyDate, Thumbnail, Width, Height, SizeByte, Deleted, Taken, CameraModel) in images)
            {
                var iId = Guid.Parse(Id ?? throw new ArgumentException($"Id for Image '{Name}' is null"));

                var i = new DiaryImage
                {
                    Id = iId,
                    Name = Name ?? "",
                    CreateDate = DateTime.Parse(CreateDate ?? "", CultureInfo.InvariantCulture),
                    ModifyDate = DateTime.Parse(ModifyDate ?? "", CultureInfo.InvariantCulture),
                    Thumbnail = Thumbnail,
                    Width = Width,
                    Height = Height,
                    SizeByte = SizeByte,
                    Deleted = Deleted,
                    Taken = Taken,
                    CameraModel = CameraModel
                };

                var oldFi = fullImages.Single(ffi => ffi.ImageId == Id);
                var fiId = Guid.Parse(oldFi.Id ?? throw new ArgumentException($"Id for full Image '{Name}' is null"));

                i.FullImage = new DiaryImageFull
                {
                    Id = fiId,
                    ImageId = i.Id,
                    Data = oldFi.Data
                };

                imgList.Add(i);
            }

            await context.Images.AddRangeAsync(imgList).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        private static async Task<List<(string Key, string Value, string ModifiedDate)>> ReadAppSettings(SqliteConnection connection)
        {
            await CheckColumnsCount(connection, "AppSettings", 3).ConfigureAwait(false);
            //
            var appSettings = new List<(string Key, string Value, string ModifiedDate)>();
            using var c5 = connection.CreateCommand();
            c5.CommandText = "SELECT * FROM AppSettings";
            using var r5 = await c5.ExecuteReaderAsync().ConfigureAwait(false);
            while (await r5.ReadAsync().ConfigureAwait(false))
            {
                appSettings.Add((
                    r5["Key"].ToString() ?? "",
                    r5["Value"].ToString() ?? "",
                    r5["ModifiedDate"].ToString() ?? ""));
            }
            await r5.CloseAsync().ConfigureAwait(false);

            return appSettings;
        }

        private static async Task WriteAppSettings(DiaryDbContext context, List<(string Key, string Value, string ModifiedDate)> appSettings)
        {
            var appSet = new List<AppSetting>();
            foreach (var (Key, Value, ModifiedDate) in appSettings)
            {
                appSet.Add(new AppSetting
                {
                    Key = Key,
                    Value = Value,
                    ModifiedDate = DateTime.Parse(ModifiedDate ?? "", CultureInfo.InvariantCulture)
                });
            }

            await context.AppSettings.AddRangeAsync(appSet).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        private static async Task<List<(string? Id, string? Date, string? CreateDate, string? ModifyDate, string? Name, string? Text, bool Deleted)>> ReadRecords(SqliteConnection connection)
        {
            await CheckColumnsCount(connection, "Records", 7).ConfigureAwait(false);
            //
            var records = new List<(string? Id, string? Date, string? CreateDate, string? ModifyDate, string? Name, string? Text, bool Deleted)>();
            using var c6 = connection.CreateCommand();
            c6.CommandText = "SELECT * FROM Records";
            using var r6 = await c6.ExecuteReaderAsync().ConfigureAwait(false);
            while (await r6.ReadAsync().ConfigureAwait(false))
            {
                records.Add((
                    r6["Id"].ToString(),
                    r6["Date"].ToString(),
                    r6["CreateDate"].ToString(),
                    r6["ModifyDate"].ToString(),
                    r6["Name"].ToString(),
                    r6["Text"].ToString(),
                    Convert.ToBoolean(r6["Deleted"], CultureInfo.InvariantCulture))
                    );
            }
            await r6.CloseAsync().ConfigureAwait(false);

            return records;
        }

        private static async Task WriteRecords(DiaryDbContext context, List<(string? Id, string? Date, string? CreateDate, string? ModifyDate, string? Name, string? Text, bool Deleted)> records)
        {
            var recordsInstances = new List<DiaryRecord>();
            foreach (var (Id, Date, CreateDate, ModifyDate, Name, Text, Deleted) in records)
            {
                var id = Guid.Parse(Id ?? throw new ArgumentException($"Id for Record '{Name}' is null"));

                var r = new DiaryRecord
                {
                    Id = id,
                    Date = DateOnly.Parse(Date ?? "", CultureInfo.InvariantCulture),
                    CreateDate = DateTime.Parse(CreateDate ?? "", CultureInfo.InvariantCulture),
                    ModifyDate = DateTime.Parse(ModifyDate ?? "", CultureInfo.InvariantCulture),
                    Name = Name ?? "",
                    Text = Text ?? "",
                    Deleted = Deleted
                };

                recordsInstances.Add(r);
            }

            await context.Records.AddRangeAsync(recordsInstances).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        private static async Task<List<(string? Id, string? RecordId, string? Date, string? Text, bool Deleted)>> ReadCogitations(SqliteConnection connection)
        {
            await CheckColumnsCount(connection, "Cogitations", 5).ConfigureAwait(false);
            //
            var cogitations = new List<(string? Id, string? RecordId, string? Date, string? Text, bool Deleted)>();
            using var c7 = connection.CreateCommand();
            c7.CommandText = "SELECT * FROM Cogitations";
            using var r7 = await c7.ExecuteReaderAsync().ConfigureAwait(false);
            while (await r7.ReadAsync().ConfigureAwait(false))
            {
                cogitations.Add((
                    r7["Id"].ToString(),
                    r7["RecordId"].ToString(),
                    r7["Date"].ToString(),
                    r7["Text"].ToString(),
                    Convert.ToBoolean(r7["Deleted"], CultureInfo.InvariantCulture))
                    );
            }
            await r7.CloseAsync().ConfigureAwait(false);

            return cogitations;
        }

        private static async Task WriteCogitations(DiaryDbContext context, List<(string? Id, string? RecordId, string? Date, string? Text, bool Deleted)> cogitations)
        {
            var cogList = new List<Cogitation>();
            foreach (var (Id, RecordId, Date, Text, Deleted) in cogitations)
            {
                var id = Guid.Parse(Id ?? throw new ArgumentException($"Id for Cogitation '{Text}' is null"));
                var recId = Guid.Parse(RecordId ?? throw new ArgumentException($"Id for Cogitation Record '{Text}' is null"));

                var c = new Cogitation
                {
                    Id = id,
                    Date = DateTime.Parse(Date ?? "", CultureInfo.InvariantCulture),
                    RecordId = recId,
                    Text = Text ?? "",
                    Deleted = Deleted
                };

                cogList.Add(c);
            }

            await context.Cogitations.AddRangeAsync(cogList).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        private static async Task<List<(string? ThemeId, string? RecordId, bool Deleted)>> ReadRecordThemes(SqliteConnection connection)
        {
            await CheckColumnsCount(connection, "RecordThemes", 3).ConfigureAwait(false);
            //
            var recThemes = new List<(string? ThemeId, string? RecordId, bool Deleted)>();
            using var c8 = connection.CreateCommand();
            c8.CommandText = "SELECT * FROM RecordThemes";
            using var r8 = await c8.ExecuteReaderAsync().ConfigureAwait(false);
            while (await r8.ReadAsync().ConfigureAwait(false))
            {
                recThemes.Add((
                    r8["ThemeId"].ToString(),
                    r8["RecordId"].ToString(),
                    Convert.ToBoolean(r8["Deleted"], CultureInfo.InvariantCulture))
                    );
            }
            await r8.CloseAsync().ConfigureAwait(false);

            return recThemes;
        }

        private static async Task WriteRecordsThemes(DiaryDbContext context, List<(string? ThemeId, string? RecordId, bool Deleted)> recThemes)
        {
            var recThemList = new List<DiaryRecordTheme>();
            foreach (var (ThemeId, RecordId, Deleted) in recThemes)
            {
                var rt = new DiaryRecordTheme
                {
                    ThemeId = Guid.Parse(ThemeId ?? ""),
                    RecordId = Guid.Parse(RecordId ?? ""),
                    Deleted = Deleted
                };

                recThemList.Add(rt);
            }

            await context.RecordThemes.AddRangeAsync(recThemList).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        private static async Task<List<(string? ImageId, string? RecordId, bool Deleted)>> ReadRecordsImages(SqliteConnection connection)
        {
            await CheckColumnsCount(connection, "RecordImages", 3).ConfigureAwait(false);
            //
            var recImages = new List<(string? ImageId, string? RecordId, bool Deleted)>();
            using var c9 = connection.CreateCommand();
            c9.CommandText = "SELECT * FROM RecordImages";
            using var r9 = await c9.ExecuteReaderAsync().ConfigureAwait(false);
            while (await r9.ReadAsync().ConfigureAwait(false))
            {
                recImages.Add((
                    r9["ImageId"].ToString(),
                    r9["RecordId"].ToString(),
                    Convert.ToBoolean(r9["Deleted"], CultureInfo.InvariantCulture))
                    );
            }
            await r9.CloseAsync().ConfigureAwait(false);

            return recImages;
        }

        private static async Task WriteRecordsImages(DiaryDbContext context, List<(string? ImageId, string? RecordId, bool Deleted)> recImages)
        {
            var recImgList = new List<DiaryRecordImage>();
            foreach (var (ImageId, RecordId, Deleted) in recImages)
            {
                var ri = new DiaryRecordImage
                {
                    ImageId = Guid.Parse(ImageId ?? ""),
                    RecordId = Guid.Parse(RecordId ?? ""),
                    Deleted = Deleted
                };

                recImgList.Add(ri);
            }

            await context.RecordImages.AddRangeAsync(recImgList).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        private static async Task CheckColumnsCount(SqliteConnection connection, string tableName, int expectedColumnsCount)
        {
            using var checkCount = connection.CreateCommand();
            checkCount.CommandText = $@"SELECT * FROM {tableName} ORDER BY ROWID ASC LIMIT 1";
            using var r = await checkCount.ExecuteReaderAsync().ConfigureAwait(false);
            await r.ReadAsync().ConfigureAwait(false);
            int columnsCount = r.FieldCount;
            await r.CloseAsync();
            if (columnsCount != expectedColumnsCount) throw new Exception($"В таблице {tableName} количество колонок не совпадает с ожидаемым");
        }
    }
}
