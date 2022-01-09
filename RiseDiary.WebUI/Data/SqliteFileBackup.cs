using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace RiseDiary.WebUI
{
    public static class SqliteFileBackup
    {
        internal static (string path, string backupsPath) GetPathAndBackupFolder(string fileNameFull)
        {
            if (fileNameFull == null) throw new ArgumentNullException(nameof(fileNameFull));
            return (Path.GetDirectoryName(fileNameFull) ?? "", Path.Combine(Path.GetDirectoryName(fileNameFull) ?? "", "backup"));
        }

        internal static void BackupFile(string fileNameFull)
        {
            if (!File.Exists(fileNameFull)) return;
            (_, string backupsPath) = GetPathAndBackupFolder(fileNameFull);
            var backupFileName = Path.Combine(backupsPath, DateTime.Now.ToString("yyyy.MM.dd - hh_mm_ss", CultureInfo.InvariantCulture) + ".bak");

            if (!Directory.Exists(backupsPath))
            {
                Directory.CreateDirectory(backupsPath);
            }

            if (!File.Exists(backupFileName))
            {
                File.Copy(fileNameFull, backupFileName);
            }
        }

        private static DiaryDbContext CreateCurrentContext(string migratedSqlite)
        {
            var connStrMigrated = $@"Data Source={migratedSqlite};";

            IConfigurationRoot configuration = new ConfigurationBuilder().Build();
            var builder = new DbContextOptionsBuilder<DiaryDbContext>();
            builder.UseSqlite(connStrMigrated);
            var context = new DiaryDbContext(builder.Options);
            context.Database.Migrate();

            return context;
        }

        public static void File2FileMigration(string fileNameFull)
        {
            // actual EF context
            var migratedFile = Path.Combine(Path.GetDirectoryName(fileNameFull) ?? "", Path.GetFileName(fileNameFull) + ".Migration." + DateTime.Now.ToString("yyyy.MM.dd_mm.ss", CultureInfo.InvariantCulture));
            using var context = CreateCurrentContext(migratedFile);

            // old SQLite file
            var connStrSource = $@"Data Source={fileNameFull};";
            using var connection = new SqliteConnection(connStrSource);
            connection.Open();

            var scopes = ReadScopes(connection);
            var themes = ReadThemes(connection);
            WriteScopesAndThemes(context, scopes, themes);

            var images = ReadImages(connection);
            var fullImages = ReadFullImages(connection);
            WriteImages(context, images, fullImages);

            var settings = ReadAppSettings(connection);
            WriteAppSettings(context, settings);

            var records = ReadRecords(connection);
            WriteRecords(context, records);

            var cogitations = ReadCogitations(connection);
            WriteCogitations(context, cogitations);

            var recThemes = ReadRecordThemes(connection);
            WriteRecordsThemes(context, recThemes);

            var recImages = ReadRecordsImages(connection);
            WriteRecordsImages(context, recImages);
        }

        private static List<(string? Id, string? Name, bool Deleted)> ReadScopes(SqliteConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Scopes";

            var scopes = new List<(string? Id, string? Name, bool Deleted)>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                scopes.Add((
                    reader["Id"].ToString(),
                    reader["ScopeName"].ToString(),
                    Convert.ToBoolean(reader["Deleted"], CultureInfo.InvariantCulture)));
            }
            reader.Close();

            return scopes;
        }

        private static List<(string? Id, string? ScopeId, string? ThemeName, bool Deleted, bool Actual)> ReadThemes(SqliteConnection connection)
        {
            var themes = new List<(string? Id, string? ScopeId, string? ThemeName, bool Deleted, bool Actual)>();
            using var c2 = connection.CreateCommand();
            c2.CommandText = "SELECT * FROM Themes";

            using var r2 = c2.ExecuteReader();
            while (r2.Read())
            {
                themes.Add((
                    r2["Id"].ToString(),
                    r2["ScopeId"].ToString(),
                    r2["ThemeName"].ToString(),
                    Convert.ToBoolean(r2["Deleted"], CultureInfo.InvariantCulture),
                    Convert.ToBoolean(r2["Actual"], CultureInfo.InvariantCulture)));
            }
            r2.Close();

            return themes;
        }

        private static void WriteScopesAndThemes(DiaryDbContext context, List<(string? Id, string? Name, bool Deleted)> scopes, List<(string? Id, string? ScopeId, string? ThemeName, bool Deleted, bool Actual)> themes)
        {
            var sList = new List<DiaryScope>();
            foreach (var (Id, Name, Deleted) in scopes)
            {
                var sId = Guid.Parse(Id ?? throw new ArgumentException($"Id for scope '{Name}' is null"));

                var s = new DiaryScope
                {
                    Id = sId,
                    ScopeName = Name ?? "",
                    Deleted = Deleted
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

            context.Scopes.AddRange(sList);
            context.SaveChanges();
        }

        private static List<(string? Id, string? Name, string? CreateDate, string? ModifyDate, byte[] Thumbnail, int Width, int Height, int SizeByte, bool Deleted, DateTime? Taken, string? cameraModel)> ReadImages(SqliteConnection connection)
        {
            var images = new List<(string? Id, string? Name, string? CreateDate, string? ModifyDate, byte[] Thumbnail, int Width, int Height, int SizeByte, bool Deleted, DateTime? Taken, string? cameraModel)>();
            using var c3 = connection.CreateCommand();
            c3.CommandText = "SELECT * FROM Images";
            using var r3 = c3.ExecuteReader();
            while (r3.Read())
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
            r3.Close();

            return images;
        }

        private static List<(string? Id, string? ImageId, byte[] Data)> ReadFullImages(SqliteConnection connection)
        {
            var fullImages = new List<(string? Id, string? ImageId, byte[] Data)>();
            using var c4 = connection.CreateCommand();
            c4.CommandText = "SELECT * FROM FullSizeImages";
            using var r4 = c4.ExecuteReader();
            while (r4.Read())
            {
                fullImages.Add((
                    r4["Id"].ToString(),
                    r4["ImageId"].ToString(),
                    (byte[])r4["Data"]));
            }
            r4.Close();

            return fullImages;
        }

        private static void WriteImages(DiaryDbContext context, List<(string? Id, string? Name, string? CreateDate, string? ModifyDate, byte[] Thumbnail, int Width, int Height, int SizeByte, bool Deleted, DateTime? Taken, string? cameraModel)> images, List<(string? Id, string? ImageId, byte[] Data)> fullImages)
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

            context.Images.AddRange(imgList);
            context.SaveChanges();
        }

        private static List<(string Key, string Value, string ModifiedDate)> ReadAppSettings(SqliteConnection connection)
        {
            var appSettings = new List<(string Key, string Value, string ModifiedDate)>();
            using var c5 = connection.CreateCommand();
            c5.CommandText = "SELECT * FROM AppSettings";
            using var r5 = c5.ExecuteReader();
            while (r5.Read())
            {
                appSettings.Add((
                    r5["Key"].ToString() ?? "",
                    r5["Value"].ToString() ?? "",
                    r5["ModifiedDate"].ToString() ?? ""));
            }
            r5.Close();

            return appSettings;
        }

        private static void WriteAppSettings(DiaryDbContext context, List<(string Key, string Value, string ModifiedDate)> appSettings)
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

            context.AppSettings.AddRange(appSet);
            context.SaveChanges();
        }

        private static List<(string? Id, string? Date, string? CreateDate, string? ModifyDate, string? Name, string? Text, bool Deleted)> ReadRecords(SqliteConnection connection)
        {
            var records = new List<(string? Id, string? Date, string? CreateDate, string? ModifyDate, string? Name, string? Text, bool Deleted)>();
            using var c6 = connection.CreateCommand();
            c6.CommandText = "SELECT * FROM Records";
            using var r6 = c6.ExecuteReader();
            while (r6.Read())
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
            r6.Close();

            return records;
        }

        private static void WriteRecords(DiaryDbContext context, List<(string? Id, string? Date, string? CreateDate, string? ModifyDate, string? Name, string? Text, bool Deleted)> records)
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

            context.Records.AddRange(recordsInstances);
            context.SaveChanges();
        }

        private static List<(string? Id, string? RecordId, string? Date, string? Text, bool Deleted)> ReadCogitations(SqliteConnection connection)
        {
            var cogitations = new List<(string? Id, string? RecordId, string? Date, string? Text, bool Deleted)>();
            using var c7 = connection.CreateCommand();
            c7.CommandText = "SELECT * FROM Cogitations";
            using var r7 = c7.ExecuteReader();
            while (r7.Read())
            {
                cogitations.Add((
                    r7["Id"].ToString(),
                    r7["RecordId"].ToString(),
                    r7["Date"].ToString(),
                    r7["Text"].ToString(),
                    Convert.ToBoolean(r7["Deleted"], CultureInfo.InvariantCulture))
                    );
            }
            r7.Close();

            return cogitations;
        }

        private static void WriteCogitations(DiaryDbContext context, List<(string? Id, string? RecordId, string? Date, string? Text, bool Deleted)> cogitations)
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

            context.Cogitations.AddRange(cogList);
            context.SaveChanges();
        }

        private static List<(string? ThemeId, string? RecordId, bool Deleted)> ReadRecordThemes(SqliteConnection connection)
        {
            var recThemes = new List<(string? ThemeId, string? RecordId, bool Deleted)>();
            using var c8 = connection.CreateCommand();
            c8.CommandText = "SELECT * FROM RecordThemes";
            using var r8 = c8.ExecuteReader();
            while (r8.Read())
            {
                recThemes.Add((
                    r8["ThemeId"].ToString(),
                    r8["RecordId"].ToString(),
                    Convert.ToBoolean(r8["Deleted"], CultureInfo.InvariantCulture))
                    );
            }
            r8.Close();

            return recThemes;
        }

        private static void WriteRecordsThemes(DiaryDbContext context, List<(string? ThemeId, string? RecordId, bool Deleted)> recThemes)
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

            context.RecordThemes.AddRange(recThemList);
            context.SaveChanges();
        }

        private static List<(string? ImageId, string? RecordId, bool Deleted)> ReadRecordsImages(SqliteConnection connection)
        {
            var recImages = new List<(string? ImageId, string? RecordId, bool Deleted)>();
            using var c9 = connection.CreateCommand();
            c9.CommandText = "SELECT * FROM RecordImages";
            using var r9 = c9.ExecuteReader();
            while (r9.Read())
            {
                recImages.Add((
                    r9["ImageId"].ToString(),
                    r9["RecordId"].ToString(),
                    Convert.ToBoolean(r9["Deleted"], CultureInfo.InvariantCulture))
                    );
            }
            r9.Close();

            return recImages;
        }

        private static void WriteRecordsImages(DiaryDbContext context, List<(string? ImageId, string? RecordId, bool Deleted)> recImages)
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

            context.RecordImages.AddRange(recImgList);
            context.SaveChanges();
        }
    }
}
