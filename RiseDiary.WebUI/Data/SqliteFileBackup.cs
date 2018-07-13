using System;
using System.IO;

namespace RiseDiary.WebUI
{
    internal static class SqliteFileBackup
    {
        internal static (string path, string backupsPath) GetPathAndBackupFolder(string fileNameFull) => 
            (Path.GetDirectoryName(fileNameFull), Path.Combine(Path.GetDirectoryName(fileNameFull), "backup"));
        
        internal static void BackupFile(string fileNameFull)
        {
            if (!File.Exists(fileNameFull)) return;
            (string path, string backupsPath) = GetPathAndBackupFolder(fileNameFull);                       
            var backupFileName = Path.Combine(backupsPath, DateTime.Now.ToString("yyyy.MM.dd - hh_mm_ss") + ".bak");
                        
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
