using System.Text.Json.Serialization;

namespace RiseDiary.Shared.Settings;

public sealed class PagesSizesSettings
{
    public int ImagesPageSize { get; set; }

    public int RecordsPageSize { get; set; }

    public int AvailableImagesPageSize { get; set; }

    public static IEnumerable<string> SettingsKeys
    {
        get
        {
            yield return nameof(AppSettingsKey.ImagesPageSize);
            yield return nameof(AppSettingsKey.RecordsPageSize);
            yield return nameof(AppSettingsKey.AvailableImagesPageSize);
        }
    }

    [JsonIgnore]
    public IEnumerable<string> PropertiesValues
    {
        get
        {
            yield return ImagesPageSize.ToString();
            yield return RecordsPageSize.ToString();
            yield return AvailableImagesPageSize.ToString();
        }
    }
}
