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
}
