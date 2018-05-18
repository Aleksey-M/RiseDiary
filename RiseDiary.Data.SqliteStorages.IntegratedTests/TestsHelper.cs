using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RiseDiary.Data.SqliteStorages.IntegratedTests.TestDomain;
using RiseDiary.Domain;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests
{
    public static class TestHelper
    {
        private static List<string> _dbFileNames = new List<string>();
        public static string DirNameFull => AppDomain.CurrentDomain.BaseDirectory;
        /*
        public static DataBaseManager GetClearBase()
        {
            var dbManager = new DataBaseManager(DirNameFull, Path.GetFileName(Path.GetTempFileName()));
            string fileNameFull = Path.Combine(DirNameFull, dbManager.DbFileName);
            if (File.Exists(fileNameFull))
            {
                File.Delete(fileNameFull);
            }
            _dbFileNames.Add(fileNameFull);
            return dbManager;
        }
        */
        public static DiaryDbContext GetClearBase()
        {
            var (context, fileName) = GetClearBaseAndFileName();
            return context;
        }

        public static (DiaryDbContext context, string fileName) GetClearBaseAndFileName()
        {
            var dbFileFullName = Path.Combine(DirNameFull, Path.GetFileName(Path.GetTempFileName()));
            if (File.Exists(dbFileFullName))
            {
                File.Delete(dbFileFullName);
            }
            _dbFileNames.Add(dbFileFullName);

            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())            
            .Build();

            var builder = new DbContextOptionsBuilder<DiaryDbContext>();
            builder.UseSqlite($"Data Source={dbFileFullName};");

            var context = new DiaryDbContext(builder.Options);
            context.Database.EnsureCreated();

            return (context, dbFileFullName);
        }

        public static void RemoveTmpDbFiles()
        {
            _dbFileNames.ForEach(fn => File.Delete(fn));
            _dbFileNames.Clear();
        }

        public static DiaryRecord GetTestRecord(int? yearsAdd = null, int? month = null)
        {
            DateTime now = DateTime.Now;
            if(yearsAdd == null)
                now = month == null ? now : new DateTime(now.Year, (int)month, 10); 
            else
                now = month == null ? now.AddYears((int)yearsAdd) : new DateTime(now.Year+(int)yearsAdd, (int)month, 10);
            
            return new DiaryRecord
            {
                RecordDate = now,
                RecordCreateDate = DateTime.Now,
                RecordModifyDate = DateTime.Now,
                RecordName = @"/-*/--!@#$+_)*&(^*^$#?>?<ЪъЇЇіячсіопдоавйцу",
                RecordText = @";'][[p][p]}{}{Pрпорпырорпорёёёё"
            };
        }

        public static (DiaryDbContext context, int recId, int cogId) GetBaseWith3RecordsAndOneCogitation()
        {
            var context = GetClearBase();
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

            return (context, recId, cogitation.Id);
        }

        //public static (DataBaseManager dbManager, int recId, int cogId) GetBaseWith3RecordsAndOneCogitation()
        //{              
        //    var mgr = GetClearBase();
        //    int recId, cogId;
        //    using (var connection = mgr.GetConnection().Result)
        //    {
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = "INSERT INTO Records ( RecordDate, RecordName, RecordText ) VALUES ( DATETIME('now'),'first','1111'), ( DATETIME('now'),'second','2222'), ( DATETIME('now'),'third','3333')";
        //            command.ExecuteNonQuery();               
        //            command.CommandText = "SELECT RecordId FROM Records WHERE RecordName='second'";
        //            using (var dr = command.ExecuteReader())
        //            {
        //                dr.Read();
        //                recId = dr.GetInt32(0);
        //            }                
        //            command.CommandText = $"INSERT INTO Cogitations (RecordId, Cogitation) VALUES ({recId}, 'COGITATION')";
        //            command.ExecuteNonQuery();
        //            command.CommandText = "SELECT CogitationId FROM Cogitations WHERE Cogitation='COGITATION'";
        //            using (var dr = command.ExecuteReader())
        //            {
        //                dr.Read();
        //                cogId = dr.GetInt32(0);
        //            }
        //        }                
        //    }
        //    return (mgr, recId, cogId);
        //}

        public static IEnumerable<string> GetNumberList(int count) => Enumerable.Range(1, count).Select(i => i.ToString("00"));
        public static IEnumerable<DateTime> GetDatesList(int count) => Enumerable.Range(1, count).Select(i => DateTime.Now.AddDays(-i).Date);
        public static IEnumerable<DateTime> GetDatesListWithTwoSameDatesWeekAgo(int count) => Enumerable.Range(1, count).Select(i => i == 2 ? DateTime.Now.AddDays(-7).Date : DateTime.Now.AddDays(-i).Date);

        public static RiseDiary.Model.DiaryRecord ToNewModel(this DiaryRecord record)
        {
            return new RiseDiary.Model.DiaryRecord
            {
                CreateDate = record.RecordCreateDate,
                Date = record.RecordDate,
                Id = record.RecordId,
                ModifyDate = record.RecordModifyDate,
                Name = record.RecordName,
                Text = record.RecordText
            };
        }

        public static DiaryRecord ToOldModel(this RiseDiary.Model.DiaryRecord record)
        {
            return new DiaryRecord
            {
                RecordId = record.Id,
                RecordCreateDate = record.CreateDate,
                RecordDate = record.Date,
                RecordModifyDate = record.ModifyDate,
                RecordName = record.Name,
                RecordText = record.Text
            };
        }

        public static IEnumerable<DiaryRecord> ReadAllRecords(DiaryDbContext context)
        {
            return context.Records.Select(r => r.ToOldModel()).ToList();
        }

        //public static IEnumerable<DiaryRecord> ReadAllRecords(DataBaseManager dbManager)
        //{
        //    using (var connection = dbManager.GetConnection().Result)
        //    using (var command = connection.CreateCommand())
        //    {
        //        command.CommandText = "SELECT * FROM Records";
        //        int counter = 0;
        //        using (var dr = command.ExecuteReader())
        //        {
        //            while (dr.Read())
        //            {
        //                if(counter == 20) yield break;
        //                counter++;
        //                var rec = new DiaryRecord
        //                {
        //                    RecordId = Convert.ToInt32(dr["RecordId"]),
        //                    RecordName = dr["RecordName"].ToString(),
        //                    RecordDate = Convert.ToDateTime(dr["RecordDate"])
        //                };
        //                yield return rec;
        //            }
        //        }
        //    }
        //}

        public static (DiaryDbContext context, IEnumerable<DiaryRecord> addedRecords) GetBaseWith20Records(IEnumerable<string> _20recordNames, IEnumerable<DateTime> _20recordDates)
        {
            if (_20recordDates.Count() != 20) throw new ArgumentOutOfRangeException(nameof(_20recordDates));
            if (_20recordNames.Count() != 20) throw new ArgumentOutOfRangeException(nameof(_20recordNames));

            var context = GetClearBase();

            var recList = _20recordNames.Select((n, i) => new DiaryRecord { RecordName = n, RecordDate = _20recordDates.ElementAt(i) });
            context.Records.AddRange(recList.Select(r => r.ToNewModel()));
            context.SaveChanges();

            return (context, ReadAllRecords(context));
        }

        //public static (DataBaseManager dbManager, IEnumerable<DiaryRecord> addedRecords) GetBaseWith20Records(IEnumerable<string> _20recordNames, IEnumerable<DateTime> _20recordDates)
        //{
        //    if (_20recordDates.Count() != 20) throw new ArgumentOutOfRangeException(nameof(_20recordDates));
        //    if (_20recordNames.Count() != 20) throw new ArgumentOutOfRangeException(nameof(_20recordNames));

        //    var mgr = GetClearBase();
        //    var recList = _20recordNames.Select((n, i) => new DiaryRecord { RecordName = n, RecordDate = _20recordDates.ElementAt(i) });
        //    using (var connection = mgr.GetConnection().Result)
        //    using (var command = connection.CreateCommand())
        //    {
        //        var sb = new StringBuilder($"INSERT INTO Records ( RecordDate, RecordName ) VALUES (@date0, @name0)");
        //        for(int i = 1; i < recList.Count(); i++)
        //        {
        //            sb.Append($", (@date{i}, @name{i})");                   
        //        }
        //        command.CommandText = sb.ToString();
        //        for(int i = 0; i < recList.Count(); i++)
        //        {
        //            command.Parameters.Add($"@date{i}", SqliteType.Text).Value = _20recordDates.ElementAt(i).ToString("o");
        //            command.Parameters.Add($"@name{i}", SqliteType.Text).Value = _20recordNames.ElementAt(i);
        //        }
        //        command.ExecuteNonQuery();
        //    }
        //    return (mgr, ReadAllRecords(mgr));
        //}

        public static (DiaryDbContext context, IEnumerable<DiaryRecord> addedRecords, IEnumerable<Model.DiaryTheme> addedThemes) GetContextWith30ThemesAnd20Records(IEnumerable<string> _20recordNames, IEnumerable<DateTime> _20recordDates)
        {
            var (context, addedRecords) = GetBaseWith20Records(_20recordNames, _20recordDates);

            var scope = new Model.DiaryScope { ScopeName = "Test" };
            context.Scopes.Add(scope);
            context.SaveChanges();

            context.Themes.AddRange(Enumerable.Range(0, 30).Select(i => new Model.DiaryTheme { DiaryScopeId = scope.Id, ThemeName = i.ToString() }));
            context.SaveChanges();

            return (context, addedRecords, context.Themes.ToList());
        }

        public static void BindRecordsWithThemes(DiaryDbContext context, Dictionary<string, List<string>> binding)
        {
            foreach(var recordTheme in binding)
            {
                foreach(var tName in recordTheme.Value)
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
        public static bool HasRecordWithIntName(List<DiaryRecord> records, int intName) => records.Any(r => int.Parse(r.RecordName) == intName);

        //public static (DataBaseManager dbManager, IEnumerable<DiaryRecord> addedRecords) GetBaseWith20RecordsAndRecordTypes(IEnumerable<string> _20recordNames, IEnumerable<DateTime> _20recordDates, int [][] _20recTypes)
        //{
        //    if (_20recTypes.Count() != 20) throw new ArgumentOutOfRangeException(nameof(_20recTypes));
        //    var savedRecordsData = GetBaseWith20Records(_20recordNames, _20recordDates);

        //    using (var connection = savedRecordsData.dbManager.GetConnection().Result)
        //    using (var command = connection.CreateCommand())
        //    {
        //        var sb = new StringBuilder("INSERT INTO TypesOfRecord VALUES (7777777, 7777777)"); // для упрощения формирования sql комманды добавляем сразу одну запись
        //        for (int i = 0; i < savedRecordsData.addedRecords.Count(); i++)
        //        {
        //            int currRecId = savedRecordsData.addedRecords.ElementAt(i).RecordId;
        //            foreach (int typeId in _20recTypes[i])
        //            {
        //                sb.Append(", (").Append(typeId).Append(", ").Append(currRecId).Append(")");
        //            }
        //        }
        //        sb.Append(";");
        //        command.CommandText = sb.ToString();
        //        command.ExecuteNonQuery();
        //    }
        //    return savedRecordsData;
        //}       
    }
}
