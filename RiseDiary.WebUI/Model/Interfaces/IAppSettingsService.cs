using RiseDiary.Shared;
using System;
using System.Threading.Tasks;

namespace RiseDiary.Model
{
    public interface IAppSettingsService
    {
        Task<(string? value, DateTime? modifiedDate)> GetAppSetting(AppSettingsKey key);

        Task<int?> GetAppSettingInt(AppSettingsKey key);

        Task UpdateAppSetting(AppSettingsKey key, string value);
    }

    public static class AppSettingsServiceExtensions
    {
        public static async Task<string> GetHostAndPort(this IAppSettingsService appSettings)
        {
            var (value, _) = await appSettings.GetAppSetting(AppSettingsKey.HostAndPort).ConfigureAwait(false);
            return value ?? "";
        }

        public static string GetHostAndPortPlaceholder(this IAppSettingsService _) => "[HOST_AND_PORT]";
    }
}
