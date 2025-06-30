namespace RiseDiary.Model;

/// <summary>
/// Сущность для хранения настроек приложения в виде строки
/// </summary>
public sealed class AppSetting
{
    public string Key { get; set; } = "";

    public string Value { get; set; } = "";

    public DateTime ModifiedDate { get; set; }
}