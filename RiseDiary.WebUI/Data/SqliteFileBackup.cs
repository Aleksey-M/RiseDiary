using System;
using System.Globalization;
using System.IO;

namespace RiseDiary.WebUI
{
    internal static class SqliteFileBackup
    {
        internal static (string path, string backupsPath) GetPathAndBackupFolder(string fileNameFull)
        {
            if (fileNameFull == null) throw new ArgumentNullException(nameof(fileNameFull));
            return (Path.GetDirectoryName(fileNameFull) ?? string.Empty, Path.Combine(Path.GetDirectoryName(fileNameFull) ?? string.Empty, "backup"));
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
    }
}
