using System;
using System.IO;

namespace RiseDiary.WebUI
{
    internal static class DailyBackups
    {
        internal static void BackupFile(string path, string fileName)
        {
            var backupsPath = Path.Combine(path, "backup");
            if (!Directory.Exists(backupsPath))
            {
                Directory.CreateDirectory(backupsPath);
            }
            var backupFileName = Path.Combine(backupsPath, DateTime.Now.ToString("yyyy_MM_dd")+".bak");
            if (!File.Exists(backupFileName))
            {
                File.Copy(Path.Combine(path, fileName), backupFileName);
            }
        }
    }
}
