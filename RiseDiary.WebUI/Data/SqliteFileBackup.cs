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
            var backupFileName = Path.Combine(backupsPath, DateTime.UtcNow.ToString("yyyy.MM.dd - hh_mm_ss", CultureInfo.InvariantCulture) + ".bak");

            if (!Directory.Exists(backupsPath))
            {
                Directory.CreateDirectory(backupsPath);
            }

            if (!File.Exists(backupFileName))
            {
                File.Copy(fileNameFull, backupFileName);
            }
        }
    }
}
