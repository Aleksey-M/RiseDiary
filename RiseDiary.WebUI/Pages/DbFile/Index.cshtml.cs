using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using RiseDiary.WebUI.Data;
using System.IO;
using System.Globalization;

namespace RiseDiary.WebUI.Pages.DbFile
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _config;
        public IndexModel(IConfiguration config)
        {
            _config = config;
        }

        public string DataBaseFileName { get; private set; } = string.Empty;
        public string DataBaseFileSize { get; private set; } = string.Empty;
        public bool MigrationOnStart { get; private set; }
        public bool BackupOnStart { get; private set; }
        public string DataBaseBackupPath { get; private set; } = string.Empty;
        public int DataBaseBackupsCount { get; private set; }
        public string DataBaseBackupsSize { get; private set; } = string.Empty;
        public bool IsSqliteUsed { get; private set; }

        public void OnGet()
        {
            IsSqliteUsed = _config.GetValue<int>("usePostgreSql") != 1;
            MigrationOnStart = _config.GetValue<int>("needMigration") > 0;
            BackupOnStart = _config.GetValue<int>("needFileBackup") > 0;
            DataBaseFileName = _config.GetValue<string>("dbFile");
            DataBaseFileSize = Math.Round(new FileInfo(DataBaseFileName).Length / 1024f / 1024f, 2).ToString(CultureInfo.InvariantCulture) + " Mb";
            (_, DataBaseBackupPath) = SqliteFileBackup.GetPathAndBackupFolder(DataBaseFileName);
            if (Directory.Exists(DataBaseBackupPath))
            {
                var dir = new DirectoryInfo(DataBaseBackupPath);
                var backupsList = dir.GetFiles();
                DataBaseBackupsCount = backupsList.Length;
                DataBaseBackupsSize = Math.Round(backupsList.Aggregate<FileInfo, long>(0, (s, fi) => s + fi.Length) / 1024f / 1024f, 2).ToString(CultureInfo.InvariantCulture) + " Mb";
            }
            else
            {
                DataBaseBackupPath += " [NOT EXISTS]";
            }
        }
    }
}