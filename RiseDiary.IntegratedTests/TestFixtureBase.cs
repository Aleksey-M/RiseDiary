using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RiseDiary.SqliteStorages.IntegratedTests
{
    // tests for DbContext
    internal class TestFixtureBase
    {
        [OneTimeTearDown]
        public void CleanUp()
        {
            RemoveTmpDbFiles();
        }

        private static List<string> _dbFileNames = new List<string>();
        protected static string DirNameFull => AppDomain.CurrentDomain.BaseDirectory;

        protected static DiaryDbContext CreateContext()
        {
            var (context, fileName) = GetContextWithFileName();
            return context;
        }

        protected static (DiaryDbContext context, string fileName) GetContextWithFileName()
        {
            var dbFileFullName = Path.Combine(DirNameFull, Path.GetFileName(Path.GetTempFileName()));
            if (File.Exists(dbFileFullName))
            {
                File.Delete(dbFileFullName);
            }
            _dbFileNames.Add(dbFileFullName);

            IConfigurationRoot configuration = new ConfigurationBuilder()
            .Build();

            var builder = new DbContextOptionsBuilder<DiaryDbContext>();
            builder.UseSqlite($"Data Source={dbFileFullName};");

            var context = new DiaryDbContext(builder.Options);
            context.Database.EnsureCreated();

            return (context, dbFileFullName);
        }

        protected static void RemoveTmpDbFiles()
        {
            _dbFileNames.ForEach(fn => File.Delete(fn));
            _dbFileNames.Clear();
        }

        protected static DiaryRecord GetTestRecord(int? yearsAdd = null, int? month = null)
        {
            DateTime now = DateTime.Now;
            if (yearsAdd == null)
                now = month == null ? now : new DateTime(now.Year, (int)month, 10);
            else
                now = month == null ? now.AddYears((int)yearsAdd) : new DateTime(now.Year + (int)yearsAdd, (int)month, 10);

            return new DiaryRecord
            {
                Date = now,
                CreateDate = DateTime.Now,
                ModifyDate = DateTime.Now,
                Name = @"/-*/--!@#$+_)*&(^*^$#?>?<ЪъЇЇіячсіопдоавйцу",
                Text = @";'][[p][p]}{}{Pрпорпырорпорёёёё"
            };
        }

        protected static int Create_Record(DiaryDbContext context)
        {
            var rec = GetTestRecord();
            context.Records.Add(rec);
            context.SaveChanges();
            return rec.Id;
        }

        protected static DiaryImage GetTestImage() => new DiaryImage
        {
            CreateDate = DateTime.Now,
            Name = Guid.NewGuid().ToString(),
            Data = new byte[1024 * 1024 * 25]
        };

        protected static int Create_Image(DiaryDbContext context)
        {
            var img = GetTestImage();
            context.Images.Add(img);
            context.SaveChanges();
            return img.Id;
        }

        protected static (int recId, int cogId) Create_3Records_1Cogitation(DiaryDbContext context)
        {
            int recId;

            context.Records.Add(new Model.DiaryRecord
            {
                Date = DateTime.Now,
                Name = "first",
                Text = "1111"
            });
            context.Records.Add(new Model.DiaryRecord
            {
                Date = DateTime.Now,
                Name = "second",
                Text = "2222"
            });
            context.Records.Add(new Model.DiaryRecord
            {
                Date = DateTime.Now,
                Name = "third",
                Text = "3333"
            });
            context.SaveChanges();
            recId = context.Records.Where(r => r.Name == "second").First().Id;

            var cogitation = new Model.Cogitation
            {
                RecordId = recId,
                Text = "COGITATION"
            };
            context.Cogitations.Add(cogitation);
            context.SaveChanges();

            return (recId, cogitation.Id);
        }

        protected static IEnumerable<string> GetNumberList(int count) => Enumerable.Range(1, count).Select(i => i.ToString("00"));
        protected static IEnumerable<DateTime> GetDatesList(int count) => Enumerable.Range(1, count).Select(i => DateTime.Now.AddDays(-i).Date);
        protected static IEnumerable<DateTime> GetDatesListWithTwoSameDatesWeekAgo(int count) => Enumerable.Range(1, count).Select(i => i == 2 ? DateTime.Now.AddDays(-7).Date : DateTime.Now.AddDays(-i).Date);

        protected static void Create_20Records(DiaryDbContext context, IEnumerable<string> _20recordNames, IEnumerable<DateTime> _20recordDates)
        {
            if (_20recordDates.Count() != 20) throw new ArgumentOutOfRangeException(nameof(_20recordDates));
            if (_20recordNames.Count() != 20) throw new ArgumentOutOfRangeException(nameof(_20recordNames));

            var recList = _20recordNames.Select((n, i) => new DiaryRecord { Name = n, Date = _20recordDates.ElementAt(i) });
            context.Records.AddRange(recList);
            context.SaveChanges();
        }

        protected static void Create_30Themes_20Records(DiaryDbContext context, IEnumerable<string> _20recordNames, IEnumerable<DateTime> _20recordDates)
        {
            Create_20Records(context, _20recordNames, _20recordDates);

            var scope = new DiaryScope { ScopeName = "Test" };
            context.Scopes.Add(scope);
            context.SaveChanges();

            context.Themes.AddRange(Enumerable.Range(0, 30).Select(i => new DiaryTheme { ScopeId = scope.Id, ThemeName = i.ToString() }));
            context.SaveChanges();
        }

        protected static void BindRecordsWithThemes(DiaryDbContext context, Dictionary<string, List<string>> binding)
        {
            foreach (var recordTheme in binding)
            {
                foreach (var tName in recordTheme.Value)
                {
                    context.RecordThemes.Add(new Model.DiaryRecordTheme
                    {
                        RecordId = context.Records.First(r => r.Name == recordTheme.Key).Id,
                        ThemeId = context.Themes.First(r => r.ThemeName == tName).Id,
                    });
                }
            }
            context.SaveChanges();
        }

        protected static bool HasRecordWithIntName(List<DiaryRecord> records, int intName) => records.Any(r => int.Parse(r.Name) == intName);

        protected static List<int> Create_3Records_3_2_1Cogitations(DiaryDbContext context)
        {
            context.Records.AddRange(GetTestRecord(), GetTestRecord(), GetTestRecord());
            context.SaveChanges();
            var resList = context.Records.OrderBy(r => r.Id).Select(r => r.Id).ToList();

            DiaryRecord rec;
            for (int i = 0; i < 3; i++)
            {
                rec = context.Records.Find(resList[i]);
                for (int j = 3; j - i > 0; j--)
                {
                    var cog = new Cogitation { Date = DateTime.Now.AddDays(-j), RecordId = rec.Id, Text = new string('+', j) };
                    context.Cogitations.Add(cog);
                    context.SaveChanges();
                }
            }
            return resList;
        }

        protected static int Create_Scope(DiaryDbContext context, string scopeName = null)
        {
            var scope = new DiaryScope { ScopeName = scopeName ?? Guid.NewGuid().ToString() };
            context.Scopes.Add(scope);
            context.SaveChanges();
            return scope.Id;
        }

        protected static List<int> Create_ThemesForScope(DiaryDbContext context, int scopeId, List<string> themeNames)
        {
            var themes = themeNames.Select(n => new DiaryTheme { ScopeId = scopeId, ThemeName = n }).ToList();
            context.Themes.AddRange(themes);
            context.SaveChanges();
            return themes.Select(t => t.Id).ToList();
        }

        protected static int Create_Theme(DiaryDbContext context, string themeName, int? scopeId = null)
        {
            if(scopeId == null)
            {
                scopeId = Create_Scope(context);
            }
            var theme = new DiaryTheme
            {
                ScopeId = (int)scopeId,
                ThemeName = themeName
            };
            context.Themes.Add(theme);
            context.SaveChanges();
            return theme.Id;
        }

        protected static void Create_3Scopes_With1ThemeForEach(DiaryDbContext context)
        {
            for (int i = 0; i < 3; i++)
            {
                var scope = new DiaryScope { ScopeName = $"Scope {i + 1}" };
                context.Scopes.Add(scope);
                context.SaveChanges();
                context.Themes.Add(new DiaryTheme { ScopeId = scope.Id, ThemeName = $"Theme For Scope {i + 1}" });
                context.SaveChanges();
            }
        }     
    }
}
