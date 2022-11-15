namespace RiseDiary.Shared;

public sealed class AppSettingDto
{
    public AppSettingsKey Key { get; set; }

    public string Value { get; set; } = "";

    public DateTime ModifiedDate { get; set; }
}
