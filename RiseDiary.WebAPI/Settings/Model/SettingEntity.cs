namespace RiseDiary.WebAPI.Settings.Model;

/// <summary>
/// Сущность для хранения настроек приложения в виде строки
/// </summary>
public sealed class SettingEntity
{
    public SettingsKey Key { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime ModifiedDate { get; set; }
}