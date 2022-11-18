using System.Text.Json.Serialization;

namespace RiseDiary.Shared.Settings;

public sealed class ImportantDaysSettings
{
    public Guid? ImportantDaysScopeId { get; set; }

    public List<KeyValuePair<Guid, string>>? ScopesSelectList { get; set; }

    public int ImportantDaysDisplayRange { get; set; }

    public static IEnumerable<string> SettingsKeys
    {
        get
        {
            yield return nameof(AppSettingsKey.ImportantDaysScopeId);
            yield return nameof(AppSettingsKey.ImportantDaysDisplayRange);
        }
    }

    [JsonIgnore]
    public IEnumerable<string> PropertiesValues
    {
        get
        {
            yield return (ImportantDaysScopeId ?? Guid.Empty).ToString();
            yield return ImportantDaysDisplayRange.ToString();
        }
    }
}
