using System.Globalization;

namespace RiseDiary.Data;

public static class SqliteFileBackup
{
    internal static (string path, string backupsPath) GetPathAndBackupFolder(string fileNameFull)
    {
        ArgumentNullException.ThrowIfNull(fileNameFull);
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
