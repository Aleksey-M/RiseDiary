using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
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
        public async Task MigrateDatabase_ToNewIdFormat()
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

            var scopes = new List<(string Id, string Name, bool Deleted)>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                scopes.Add((
                    reader["Id"].ToString(),
                    reader["ScopeName"].ToString(),
                    Convert.ToBoolean(reader["Deleted"])));
            }
            reader.Close();

            var themes = new List<(string Id, string ScopeId, string ThemeName, bool Deleted, bool Actual)>();
            using var c2 = connection.CreateCommand();
            c2.CommandText = "SELECT * FROM Themes";
            using var r2 = c2.ExecuteReader();
            while (r2.Read())
            {
                themes.Add((
                    r2["Id"].ToString(),
                    r2["ScopeId"].ToString(),
                    r2["ThemeName"].ToString(),
                    Convert.ToBoolean(r2["Deleted"]),
                    Convert.ToBoolean(r2["Actual"]))
                    );
            }
            r2.Close();

            // Scopes Map
            var scopesMap = new Dictionary<string, Guid>();
            var themesMap = new Dictionary<string, Guid>();

            // write Scopes
            var sList = new List<DiaryScope>();
            foreach (var scope in scopes)
            {
                var s = new DiaryScope
                {
                    Id = Guid.NewGuid(),
                    ScopeName = scope.Name,
                    Deleted = scope.Deleted
                };

                scopesMap[scope.Id] = s.Id;

                foreach (var theme in themes.Where(t => t.ScopeId == scope.Id))
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
            var images = new List<(string Id, string Name, string CreateDate, string ModifyDate, byte[] Thumbnail, int Width, int Height, int SizeByte, bool Deleted)>();
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
                    Convert.ToInt32(r3["Width"]),
                    Convert.ToInt32(r3["Height"]),
                    Convert.ToInt32(r3["SizeByte"]),
                    Convert.ToBoolean(r3["Deleted"]))
                    );
            }
            r3.Close();
            //
            var fullImages = new List<(string Id, string ImageId, byte[] Data)>();
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
            foreach (var img in images)
            {
                var i = new DiaryImage
                {
                    Id = Guid.NewGuid(),
                    Name = img.Name,
                    CreateDate = DateTime.Parse(img.CreateDate),
                    ModifyDate = DateTime.Parse(img.ModifyDate),
                    Thumbnail = img.Thumbnail,
                    Width = img.Width,
                    Height = img.Height,
                    SizeByte = img.SizeByte,
                    Deleted = img.Deleted
                };

                imagesMap[img.Id] = i.Id;

                var fi = new DiaryImageFull
                {
                    Id = Guid.NewGuid(),
                    ImageId = i.Id,
                    Data = fullImages.First(ffi => ffi.ImageId == img.Id).Data,
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
                    Key = r5["Key"].ToString(),
                    Value = r5["Key"].ToString() == "ImportantDaysScopeId" ? scopesMap[r5["Value"].ToString()].ToString() : r5["Value"].ToString(),
                    ModifiedDate = DateTime.Parse(r5["ModifiedDate"].ToString())
                };

                context.AppSettings.Add(appS);
            }
            r5.Close();
            await context.SaveChangesAsync();


            // Records Map
            var recordsMap = new Dictionary<string, Guid>();

            // Read records
            var records = new List<(string Id, string Date, string CreateDate, string ModifyDate, string Name, string Text, bool Deleted)>();
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
                    Convert.ToBoolean(r6["Deleted"]))
                    );
            }
            r6.Close();

            // create records instances
            var recordsInstances = new List<DiaryRecord>();
            foreach (var rec in records)
            {
                var r = new DiaryRecord
                {
                    Id = Guid.NewGuid(),
                    Date = DateTime.Parse(rec.Date),
                    CreateDate = DateTime.Parse(rec.CreateDate),
                    ModifyDate = DateTime.Parse(rec.ModifyDate),
                    Name = rec.Name,
                    Text = rec.Text,
                    Deleted = rec.Deleted
                };

                recordsMap[rec.Id] = r.Id;
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

                    newLink = link.Replace(host, "[HOST_AND_PORT]");

                    // local links to records
                    int rCurrent = link.IndexOf("recordId=", 0, StringComparison.OrdinalIgnoreCase);
                    if (rCurrent > -1)
                    {
                        var recId = link.Substring(rCurrent + 9);
                        var newRecId = recordsMap[recId];
                        newLink = newLink.Replace(recId, newRecId.ToString());

                        srcText = srcText.Replace(link, newLink);
                    }

                    // local inks to images
                    int iCurrent = link.IndexOf(@"ImageFile/", 0, StringComparison.OrdinalIgnoreCase);
                    if (iCurrent > -1)
                    {
                        var imgId = link.Substring(iCurrent + 10);
                        var newImgId = imagesMap[imgId];
                        newLink = newLink.Replace(imgId, newImgId.ToString());

                        srcText = srcText.Replace(link, newLink);
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
            var cogitations = new List<(string Id, string RecordId, string Date, string Text, bool Deleted)>();
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
                    Convert.ToBoolean(r7["Deleted"]))
                    );
            }
            r7.Close();

            // write cogitations
            var cogList = new List<Cogitation>();
            foreach (var cog in cogitations)
            {
                var c = new Cogitation
                {
                    Id = Guid.NewGuid(),
                    Date = DateTime.Parse(cog.Date),
                    RecordId = recordsMap[cog.RecordId],
                    Text = ReplaceIdAndHostInText(cog.Text),
                    Deleted = cog.Deleted
                };

                cogList.Add(c);
            }

            context.Cogitations.AddRange(cogList);
            await context.SaveChangesAsync();

            // read RecortThemes
            var recThemes = new List<(string ThemeId, string RecordId, bool Deleted)>();
            using var c8 = connection.CreateCommand();
            c8.CommandText = "SELECT * FROM RecordThemes";
            using var r8 = c8.ExecuteReader();
            while (r8.Read())
            {
                recThemes.Add((
                    r8["ThemeId"].ToString(),
                    r8["RecordId"].ToString(),
                    Convert.ToBoolean(r8["Deleted"]))
                    );
            }
            r8.Close();

            // write RecordThemes

            var recThemList = new List<DiaryRecordTheme>();
            foreach (var recThem in recThemes)
            {
                var rt = new DiaryRecordTheme
                {
                    ThemeId = themesMap[recThem.ThemeId],
                    RecordId = recordsMap[recThem.RecordId],
                    Deleted = recThem.Deleted
                };

                recThemList.Add(rt);
            }

            context.RecordThemes.AddRange(recThemList);
            await context.SaveChangesAsync();

            // read RecortImages
            var recImages = new List<(string ImageId, string RecordId, bool Deleted)>();
            using var c9 = connection.CreateCommand();
            c9.CommandText = "SELECT * FROM RecordImages";
            using var r9 = c9.ExecuteReader();
            while (r9.Read())
            {
                recImages.Add((
                    r9["ImageId"].ToString(),
                    r9["RecordId"].ToString(),
                    Convert.ToBoolean(r9["Deleted"]))
                    );
            }
            r9.Close();

            // write RecordImages

            var recImgList = new List<DiaryRecordImage>();
            foreach (var recImg in recImages)
            {
                var ri = new DiaryRecordImage
                {
                    ImageId = imagesMap[recImg.ImageId],
                    RecordId = recordsMap[recImg.RecordId],
                    Deleted = recImg.Deleted
                };

                recImgList.Add(ri);
            }

            context.RecordImages.AddRange(recImgList);
            await context.SaveChangesAsync();
        }
    }
}
