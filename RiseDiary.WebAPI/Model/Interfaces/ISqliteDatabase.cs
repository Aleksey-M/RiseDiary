namespace RiseDiary.Model;

public record DeletedEntitiesCount(int Scopes, int Themes, int Records, int Cogitations, int Images, int RecordThemes, int RecordImages);

public record SqliteDatabaseFileInfo(string FileName, string FileSize);

public record DeletedRecord(Guid Id, DateOnly Date, string Name, string Text);

public record DeletedRecordTheme(Guid RecordId, Guid ThemeId, string RecordName, string ThemeName);

public record DeletedRecordImage(Guid RecordId, Guid ImageId, string RecordName, string ImageThumbnail);

public record DeletedCogitation(Guid CogitationId, Guid RecordId, DateTime Date, string Text, string RecordName);

public record DeletedImage(Guid Id, string Name, string Thumbnail);

public record DeletedScope(Guid Id, string Name);

public record DeletedTheme(Guid Id, string ScopeName, string ThemeName);

public sealed class DeletedData
{
    public IEnumerable<DeletedRecord> Records { get; set; } = Enumerable.Empty<DeletedRecord>();

    public IEnumerable<DeletedRecordImage> RecordsImages { get; set; } = Enumerable.Empty<DeletedRecordImage>();

    public IEnumerable<DeletedRecordTheme> RecordsThemes { get; set; } = Enumerable.Empty<DeletedRecordTheme>();

    public IEnumerable<DeletedCogitation> RecordsCogitations { get; set; } = Enumerable.Empty<DeletedCogitation>();

    public IEnumerable<DeletedImage> Images { get; set; } = Enumerable.Empty<DeletedImage>();

    public IEnumerable<DeletedScope> Scopes { get; set; } = Enumerable.Empty<DeletedScope>();

    public IEnumerable<DeletedTheme> Themes { get; set; } = Enumerable.Empty<DeletedTheme>();
}

public interface ISqliteDatabase
{
    Task<DeletedEntitiesCount> GetDeletedEntitiesCount(CancellationToken cancellationToken = default);

    SqliteDatabaseFileInfo GetSqliteDatabaseInfo();

    Task ClearDatabase();

    Task Vacuum();

    Task File2FileMigration();

    Task<DeletedData> GetDeletedEntitiesData(CancellationToken cancellationToken = default);
}
