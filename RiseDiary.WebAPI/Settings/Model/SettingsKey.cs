namespace RiseDiary.WebAPI.Settings.Model;

public enum SettingsKey
{
    Unknown = 0,

    // ImportantDaysSettings
    ImportantDaysScopeId = 1,
    ImportantDaysDisplayRange = 2,

    // BookmarksSettings
    BookmarksRecordsList = 3,

    // ImagesSettings
    ImageQuality = 4,
    ThumbnailSize = 5,

    // PagesSizesSettings
    RecordsPageSize = 7,
    ImagesPageSize = 8,
    AvailableImagesPageSize = 9
}
