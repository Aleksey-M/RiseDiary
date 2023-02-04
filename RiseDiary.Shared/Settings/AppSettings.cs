namespace RiseDiary.Shared;

public enum AppSettingsKey
{
    Unknown = 0,
    ImportantDaysScopeId = 1,
    ImportantDaysDisplayRange = 2,
    StartPageRecordId = 3,
    ImageQuality = 4,
    ThumbnailSize = 5,
    RecordsPageSize = 7,
    ImagesPageSize = 8,
    AvailableImagesPageSize = 9
}


public sealed class AppSettingDto
{
    public AppSettingsKey Key { get; set; }

    public string Value { get; set; } = "";

    public DateTime ModifiedDate { get; set; }
}