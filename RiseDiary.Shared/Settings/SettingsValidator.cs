namespace RiseDiary.Shared;

public static class SettingsValidator
{
    public static string Validate(AppSettingsKey key, string value) => (key, value) switch
    {
        (_, null) => "Value should not be null or empty",
        (AppSettingsKey.Unknown, _) => "Unknown settings key",

        (AppSettingsKey.ImportantDaysScopeId, _) when value != "" && !Guid.TryParse(value, out _) => "ScopeId value is not a correct GUID",

        (AppSettingsKey.ImportantDaysDisplayRange, _) when !int.TryParse(value, out _) => "Range value should be set as int",
        (AppSettingsKey.ImportantDaysDisplayRange, _) => int.Parse(value) <= 1 ? "Range value must be greater than 1" :
                                                         int.Parse(value) > 30 ? "Range value must be less than 30" : "",
        (AppSettingsKey.StartPageRecordId, _) when value != "" && !Guid.TryParse(value, out _) => "RecordId value is not a correct GUID",

        (AppSettingsKey.ImageQuality, _) when !int.TryParse(value, out _) => "Image Quality value should be set as int",
        (AppSettingsKey.ImageQuality, _) => int.Parse(value) <= 0 ? "Image Quality value must be greater than 0" :
                                                         int.Parse(value) > 100 ? "Image Quality value must be less than 100" : "",

        (AppSettingsKey.ThumbnailSize, _) when !int.TryParse(value, out _) => "Value should be set as int",
        (AppSettingsKey.ThumbnailSize, _) => int.Parse(value) <= 50 ? "Thumbnail Size value must be greater than 50" :
                                                         int.Parse(value) > 500 ? "Thumbnail Size value must be less than 500" : "",

        (AppSettingsKey.CropImageMaxScaledWidth, _) when !int.TryParse(value, out _) => "Value should be set as int",
        (AppSettingsKey.CropImageMaxScaledWidth, _) => int.Parse(value) <= 200 ? "Max Scaled Width value must be greater than 200" :
                                                         int.Parse(value) > 2000 ? "Max Scaled Width value must be less than 2000" : "",

        (AppSettingsKey.CropImageMaxScaledHeight, _) when !int.TryParse(value, out _) => "Value should be set as int",
        (AppSettingsKey.CropImageMaxScaledHeight, _) => int.Parse(value) <= 200 ? "Max Scaled Height value must be greater than 200" :
                                                         int.Parse(value) > 2000 ? "Max Scaled Height value must be less than 2000" : "",

        (AppSettingsKey.RecordsPageSize, _) when !int.TryParse(value, out _) => "Range value should be set as int",
        (AppSettingsKey.RecordsPageSize, _) => int.Parse(value) <= 1 ? "Range value must be greater than 1" :
                                               int.Parse(value) > 200 ? "Range value must be less than 200" : "",

        (AppSettingsKey.ImagesPageSize, _) when !int.TryParse(value, out _) => "Range value should be set as int",
        (AppSettingsKey.ImagesPageSize, _) => int.Parse(value) <= 1 ? "Range value must be greater than 1" :
                                                         int.Parse(value) > 200 ? "Range value must be less than 200" : "",
        (AppSettingsKey.AvailableImagesPageSize, _) when !int.TryParse(value, out _) => "Range value should be set as int",
        (AppSettingsKey.AvailableImagesPageSize, _) => int.Parse(value) <= 1 ? "Range value must be greater than 1" :
                                                         int.Parse(value) > 200 ? "Range value must be less than 200" : "",

        (_, _) => ""
    };
}