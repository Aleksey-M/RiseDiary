using RiseDiary.Shared;

namespace RiseDiary.WebAPI.Caching;

public static class CacheTags
{
    public const string ScopesNames = "scopes_names";
    public static string AppSettingKey(AppSettingsKey key) => $"setting_{key}";
    public static string CalendarItems(int year) => $"calendar_items_{year}";
    public const string CalendarYears = "calendar_years";
}
