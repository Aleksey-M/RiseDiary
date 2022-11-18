namespace RiseDiary.Shared.Database;

public record DeletedEntitiesCount(int Scopes, int Themes, int Records, int Cogitations, int Images, int RecordThemes, int RecordImages);

public record SqliteDatabaseFileInfo(string FileName, string FileSize);


public sealed class SqliteDbInfoDto
{
    public string FileName { get; set; } = null!;

    public string FileSize { get; set; } = null!;

    public int DeletedScopes { get; set; }

    public int DeletedThemes { get; set; }

    public int DeletedRecords { get; set; }

    public int DeletedCogitations { get; set; }

    public int DeletedImages { get; set; }

    public int DeletedRecordThemes { get; set; }

    public int DeletedRecordImages { get; set; }
}
