namespace RiseDiary.WebAPI.Settings.Model;

public interface ISettings
{
}

public record BookmarksSettings(
    Guid[] Records,
    DateTime ModifiedDateUtc) : ISettings;

public record ImagesSettings(
    int ImageQuality,
    int ThumbnailSize,
    DateTime ModifiedDateUtc) : ISettings;

public record ImportantDaysSettings(
    Guid? ImportantDaysScopeId,
    int ImportantDaysDisplayRange,
    DateTime ModifiedDateUtc) : ISettings;

public record PagesSizesSettings(
    int ImagesPageSize,
    int RecordsPageSize,
    int AvailableImagesPageSize,
    DateTime ModifiedDateUtc) : ISettings;