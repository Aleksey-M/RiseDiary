using System;
using System.Collections.Generic;
using System.IO;

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
    }
}
