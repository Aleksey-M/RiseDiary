using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Migrations
{
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning disable CA1303 // Do not pass literals as localized parameters
    [TestFixture]
    public class OneTimeMigrations
    {
        [Test, Ignore("Special test")]
        public async Task MigrateDatabaseToNewIdFormat()
        {
            var sourceSqlite = @"D:\Projects\RiseDiary\DB\MigrateDatabaseToNewId\Source.db";
            var migratedSqlite = @"D:\Projects\RiseDiary\DB\MigrateDatabaseToNewId\RiseDiary.db";

            var connStrSource = $@"Data Source={sourceSqlite};";
            var connStrMigrated = $@"Data Source={migratedSqlite};";

            // new database with migrated data
            if (File.Exists(migratedSqlite))
            {
                File.Delete(migratedSqlite);
            }
            IConfigurationRoot configuration = new ConfigurationBuilder().Build();
            var builder = new DbContextOptionsBuilder<DiaryDbContext>();
            builder.UseSqlite(connStrMigrated);
            using var context = new DiaryDbContext(builder.Options);
            context.Database.Migrate();

            using var connection = new SqliteConnection(connStrSource);
            connection.Open();

            // read Scopes
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
                    Convert.ToBoolean(r2["Actual"], CultureInfo.InvariantCulture))
                    );
            }
            r2.Close();

            // Scopes Map
            var scopesMap = new Dictionary<string, Guid>();
            var themesMap = new Dictionary<string, Guid>();

            // write Scopes
            var sList = new List<DiaryScope>();
            foreach (var (Id, Name, Deleted) in scopes)
            {
                var s = new DiaryScope
                {
                    Id = Guid.NewGuid(),
                    ScopeName = Name ?? string.Empty,
                    Deleted = Deleted
                };

                scopesMap[Id ?? string.Empty] = s.Id;

                foreach (var theme in themes.Where(t => t.ScopeId == Id))
                {
                    var t = new DiaryTheme
                    {
                        Id = Guid.NewGuid(),
                        ScopeId = s.Id,
                        ThemeName = theme.ThemeName,
                        Deleted = theme.Deleted,
                        Actual = theme.Actual
                    };

                    themesMap[theme.Id] = t.Id;

                    s.Themes.Add(t);
                }

                sList.Add(s);
            }

            context.Scopes.AddRange(sList);
            await context.SaveChangesAsync();

            // read images
            var images = new List<(string? Id, string? Name, string? CreateDate, string? ModifyDate, byte[] Thumbnail, int Width, int Height, int SizeByte, bool Deleted)>();
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
                    Convert.ToBoolean(r3["Deleted"], CultureInfo.InvariantCulture))
                    );
            }
            r3.Close();
            //
            var fullImages = new List<(string? Id, string? ImageId, byte[] Data)>();
            using var c4 = connection.CreateCommand();
            c4.CommandText = "SELECT * FROM FullSizeImages";
            using var r4 = c4.ExecuteReader();
            while (r4.Read())
            {
                fullImages.Add((
                    r4["Id"].ToString(),
                    r4["ImageId"].ToString(),
                    (byte[])r4["Data"])
                    );
            }
            r4.Close();

            // Images Map
            var imagesMap = new Dictionary<string, Guid>();

            // write Images
            var imgList = new List<DiaryImageFull>();
            foreach (var (Id, Name, CreateDate, ModifyDate, Thumbnail, Width, Height, SizeByte, Deleted) in images)
            {
                var i = new DiaryImage
                {
                    Id = Guid.NewGuid(),
                    Name = Name ?? string.Empty,
                    CreateDate = DateTime.Parse(CreateDate ?? string.Empty, CultureInfo.InvariantCulture),
                    ModifyDate = DateTime.Parse(ModifyDate ?? string.Empty, CultureInfo.InvariantCulture),
                    Thumbnail = Thumbnail,
                    Width = Width,
                    Height = Height,
                    SizeByte = SizeByte,
                    Deleted = Deleted
                };

                imagesMap[Id ?? string.Empty] = i.Id;

                var fi = new DiaryImageFull
                {
                    Id = Guid.NewGuid(),
                    ImageId = i.Id,
                    Data = fullImages.First(ffi => ffi.ImageId == Id).Data,
                    DiaryImage = i
                };

                imgList.Add(fi);
            }

            context.FullSizeImages.AddRange(imgList);
            await context.SaveChangesAsync();

            // AppSettings
            using var c5 = connection.CreateCommand();
            c5.CommandText = "SELECT * FROM AppSettings";
            using var r5 = c5.ExecuteReader();
            while (r5.Read())
            {
                var appS = new AppSetting
                {
                    Key = r5["Key"].ToString() ?? string.Empty,
                    Value = r5["Key"].ToString() == "ImportantDaysScopeId" ? scopesMap[r5["Value"].ToString() ?? string.Empty].ToString() ?? string.Empty : r5["Value"].ToString() ?? string.Empty,
                    ModifiedDate = DateTime.Parse(r5["ModifiedDate"].ToString() ?? string.Empty, CultureInfo.InvariantCulture)
                };

                context.AppSettings.Add(appS);
            }
            r5.Close();
            await context.SaveChangesAsync();


            // Records Map
            var recordsMap = new Dictionary<string, Guid>();

            // Read records
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

            // create records instances
            var recordsInstances = new List<DiaryRecord>();
            foreach (var (Id, Date, CreateDate, ModifyDate, Name, Text, Deleted) in records)
            {
                var r = new DiaryRecord
                {
                    Id = Guid.NewGuid(),
                    Date = DateTime.Parse(Date ?? string.Empty, CultureInfo.InvariantCulture),
                    CreateDate = DateTime.Parse(CreateDate ?? string.Empty, CultureInfo.InvariantCulture),
                    ModifyDate = DateTime.Parse(ModifyDate ?? string.Empty, CultureInfo.InvariantCulture),
                    Name = Name ?? string.Empty,
                    Text = Text ?? string.Empty,
                    Deleted = Deleted
                };

                recordsMap[Id ?? string.Empty] = r.Id;
                recordsInstances.Add(r);
            }

            //
            string host = @"http://localhost:5000";
            string ReplaceIdAndHostInText(string srcText)
            {
                int start = 0, end, current;
                string link, newLink;

                current = srcText.IndexOf(host, start, StringComparison.OrdinalIgnoreCase);
                while (current > -1)
                {
                    end = srcText.IndexOf('"', current);
                    link = srcText[current..end];

                    newLink = link.Replace(host, "[HOST_AND_PORT]", StringComparison.OrdinalIgnoreCase);

                    // local links to records
                    int rCurrent = link.IndexOf("recordId=", 0, StringComparison.OrdinalIgnoreCase);
                    if (rCurrent > -1)
                    {
                        var recId = link.Substring(rCurrent + 9);
                        var newRecId = recordsMap[recId];
                        newLink = newLink.Replace(recId, newRecId.ToString(), StringComparison.OrdinalIgnoreCase);

                        srcText = srcText.Replace(link, newLink, StringComparison.OrdinalIgnoreCase);
                    }

                    // local inks to images
                    int iCurrent = link.IndexOf(@"ImageFile/", 0, StringComparison.OrdinalIgnoreCase);
                    if (iCurrent > -1)
                    {
                        var imgId = link.Substring(iCurrent + 10);
                        var newImgId = imagesMap[imgId];
                        newLink = newLink.Replace(imgId, newImgId.ToString(), StringComparison.OrdinalIgnoreCase);

                        srcText = srcText.Replace(link, newLink, StringComparison.OrdinalIgnoreCase);
                    }

                    start = current + link.Length;
                    current = srcText.IndexOf(host, start, StringComparison.OrdinalIgnoreCase);
                }

                return srcText;
            }

            // update local links in records
            foreach (var rec in recordsInstances)
            {
                rec.Text = ReplaceIdAndHostInText(rec.Text);
            }

            // write records
            context.Records.AddRange(recordsInstances);
            await context.SaveChangesAsync();

            // read Cogitations
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

            // write cogitations
            var cogList = new List<Cogitation>();
            foreach (var (Id, RecordId, Date, Text, Deleted) in cogitations)
            {
                var c = new Cogitation
                {
                    Id = Guid.NewGuid(),
                    Date = DateTime.Parse(Date ?? string.Empty, CultureInfo.InvariantCulture),
                    RecordId = recordsMap[RecordId ?? string.Empty],
                    Text = ReplaceIdAndHostInText(Text ?? string.Empty),
                    Deleted = Deleted
                };

                cogList.Add(c);
            }

            context.Cogitations.AddRange(cogList);
            await context.SaveChangesAsync();

            // read RecortThemes
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

            // write RecordThemes

            var recThemList = new List<DiaryRecordTheme>();
            foreach (var (ThemeId, RecordId, Deleted) in recThemes)
            {
                var rt = new DiaryRecordTheme
                {
                    ThemeId = themesMap[ThemeId ?? string.Empty],
                    RecordId = recordsMap[RecordId ?? string.Empty],
                    Deleted = Deleted
                };

                recThemList.Add(rt);
            }

            context.RecordThemes.AddRange(recThemList);
            await context.SaveChangesAsync();

            // read RecortImages
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

            // write RecordImages

            var recImgList = new List<DiaryRecordImage>();
            foreach (var (ImageId, RecordId, Deleted) in recImages)
            {
                var ri = new DiaryRecordImage
                {
                    ImageId = imagesMap[ImageId ?? string.Empty],
                    RecordId = recordsMap[RecordId ?? string.Empty],
                    Deleted = Deleted
                };

                recImgList.Add(ri);
            }

            context.RecordImages.AddRange(recImgList);
            await context.SaveChangesAsync();
        }
    }
}
