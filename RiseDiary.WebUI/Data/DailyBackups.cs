using System;
using System.IO;

namespace RiseDiary.WebUI
{
    internal static class DailyBackups
    {
        internal static void BackupFile(string fileNameFull)
        {
            if (!File.Exists(fileNameFull)) return;
            var path = Path.GetDirectoryName(fileNameFull);
            var backupsPath = Path.Combine(path, "backup");            
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
