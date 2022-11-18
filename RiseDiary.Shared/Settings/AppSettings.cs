namespace RiseDiary.Shared;

public enum AppSettingsKey
{
    Unknown,
    ImportantDaysScopeId,
    ImportantDaysDisplayRange,
    StartPageRecordId,
    ImageQuality,
    CropImageMaxScaledWidth,
    CropImageMaxScaledHeight,
    ThumbnailSize,
    HostAndPort,
    RecordsPageSize,
    ImagesPageSize,
    AvailableImagesPageSize
}


public sealed class AppSettingDto
{
    public AppSettingsKey Key { get; set; }

    public string Value { get; set; } = "";

    public DateTime ModifiedDate { get; set; }
}