using System.Text.Json.Serialization;

namespace RiseDiary.Shared.Settings;

public sealed class ImagesSettings
{
    public int ImageQuality { get; set; }

    public int ThumbnailSize { get; set; }

    public int CropImageMaxScaledHeight { get; set; }

    public int CropImageMaxScaledWidth { get; set; }

    public static IEnumerable<string> SettingsKeys
    {
        get
        {
            yield return nameof(AppSettingsKey.CropImageMaxScaledHeight);
            yield return nameof(AppSettingsKey.CropImageMaxScaledWidth);
            yield return nameof(AppSettingsKey.ImageQuality);
            yield return nameof(AppSettingsKey.ThumbnailSize);
        }
    }

    [JsonIgnore]
    public IEnumerable<string> PropertiesValues
    {
        get
        {
            yield return CropImageMaxScaledHeight.ToString();
            yield return CropImageMaxScaledWidth.ToString();
            yield return ImageQuality.ToString();
            yield return ThumbnailSize.ToString();
        }
    }
}
