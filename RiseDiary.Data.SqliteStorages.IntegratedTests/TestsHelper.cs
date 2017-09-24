using Microsoft.Data.Sqlite;
using RiseDiary.Domain.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests
{
    public static class TestsHelper
    {
        private static List<string> _dbFileNames = new List<string>();
        public static string DirNameFull => AppDomain.CurrentDomain.BaseDirectory;
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

        public static (DataBaseManager dbManager, int recId, int cogId) GetBaseWith3RecordsAndOneCogitation()
        {              
            var mgr = GetClearBase();
            int recId, cogId;
            using (var connection = mgr.GetConnection().Result)
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Records ( RecordDate, RecordName, RecordText ) VALUES ( DATETIME('now'),'first','1111'), ( DATETIME('now'),'second','2222'), ( DATETIME('now'),'third','3333')";
                    command.ExecuteNonQuery();               
                    command.CommandText = "SELECT RecordId FROM Records WHERE RecordName='second'";
                    using (var dr = command.ExecuteReader())
                    {
                        dr.Read();
                        recId = dr.GetInt32(0);
                    }                
                    command.CommandText = $"INSERT INTO Cogitations (RecordId, Cogitation) VALUES ({recId}, 'COGITATION')";
                    command.ExecuteNonQuery();
                    command.CommandText = "SELECT CogitationId FROM Cogitations WHERE Cogitation='COGITATION'";
                    using (var dr = command.ExecuteReader())
                    {
                        dr.Read();
                        cogId = dr.GetInt32(0);
                    }
                }                
            }
            return (mgr, recId, cogId);
        }

        public static IEnumerable<string> GetNumberList(int count) => Enumerable.Range(1, count).Select(i => i.ToString("00"));
        public static IEnumerable<DateTime> GetDatesList(int count) => Enumerable.Range(1, count).Select(i => DateTime.Now.AddDays(-i).Date);
        public static IEnumerable<DateTime> GetDatesListWithTwoSameDatesWeekAgo(int count) => Enumerable.Range(1, count).Select(i => i == 2 ? DateTime.Now.AddDays(-7).Date : DateTime.Now.AddDays(-i).Date);

        public static IEnumerable<DiaryRecord> ReadAllRecords(DataBaseManager dbManager)
        {
            using (var connection = dbManager.GetConnection().Result)
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Records";
                int counter = 0;
                using (var dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        if(counter == 20) yield break;
                        counter++;
                        var rec = new DiaryRecord
                        {
                            RecordId = Convert.ToInt32(dr["RecordId"]),
                            RecordName = dr["RecordName"].ToString(),
                            RecordDate = Convert.ToDateTime(dr["RecordDate"])
                        };
                        yield return rec;
                    }
                }
            }
        }

        public static (DataBaseManager dbManager, IEnumerable<DiaryRecord> addedRecords) GetBaseWith20Records(IEnumerable<string> _20recordNames, IEnumerable<DateTime> _20recordDates)
        {
            if (_20recordDates.Count() != 20) throw new ArgumentOutOfRangeException(nameof(_20recordDates));
            if (_20recordNames.Count() != 20) throw new ArgumentOutOfRangeException(nameof(_20recordNames));

            var mgr = GetClearBase();
            var recList = _20recordNames.Select((n, i) => new DiaryRecord { RecordName = n, RecordDate = _20recordDates.ElementAt(i) });
            using (var connection = mgr.GetConnection().Result)
            using (var command = connection.CreateCommand())
            {
                var sb = new StringBuilder($"INSERT INTO Records ( RecordDate, RecordName ) VALUES (@date0, @name0)");
                for(int i = 1; i < recList.Count(); i++)
                {
                    sb.Append($", (@date{i}, @name{i})");                   
                }
                command.CommandText = sb.ToString();
                for(int i = 0; i < recList.Count(); i++)
                {
                    command.Parameters.Add($"@date{i}", SqliteType.Text).Value = _20recordDates.ElementAt(i).ToString("o");
                    command.Parameters.Add($"@name{i}", SqliteType.Text).Value = _20recordNames.ElementAt(i);
                }
                command.ExecuteNonQuery();
            }
            return (mgr, ReadAllRecords(mgr));
        }
        
        public static (DataBaseManager dbManager, IEnumerable<DiaryRecord> addedRecords) GetBaseWith20RecordsAndRecordTypes(IEnumerable<string> _20recordNames, IEnumerable<DateTime> _20recordDates, int [][] _20recTypes)
        {
            if (_20recTypes.Count() != 20) throw new ArgumentOutOfRangeException(nameof(_20recTypes));
            var savedRecordsData = GetBaseWith20Records(_20recordNames, _20recordDates);

            using (var connection = savedRecordsData.dbManager.GetConnection().Result)
            using (var command = connection.CreateCommand())
            {
                var sb = new StringBuilder("INSERT INTO TypesOfRecord VALUES (7777777, 7777777)"); // для упрощения формирования sql комманды добавляем сразу одну запись
                for (int i = 0; i < savedRecordsData.addedRecords.Count(); i++)
                {
                    int currRecId = savedRecordsData.addedRecords.ElementAt(i).RecordId;
                    foreach (int typeId in _20recTypes[i])
                    {
                        sb.Append(", (").Append(typeId).Append(", ").Append(currRecId).Append(")");
                    }
                }
                sb.Append(";");
                command.CommandText = sb.ToString();
                command.ExecuteNonQuery();
            }
            return savedRecordsData;
        }        
    }
}
