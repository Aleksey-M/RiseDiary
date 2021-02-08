using RiseDiary.Model;
using RiseDiary.Shared;
using System;
using System.Threading.Tasks;

namespace RiseDiary.IntegratedTests.Stubs
{
    internal class AppSettingsServiceStub : IAppSettingsService
    {
        public Task<(string? value, DateTime? modifiedDate)> GetAppSetting(AppSettingsKey key) => key switch
        {
            AppSettingsKey.HostAndPort => Task.FromResult<(string?, DateTime?)>(("https://testsite.com:3000", null)),
            _ => throw new Exception($"The stub does not know what value to return for '{key}'")
        };

        public Task<int?> GetAppSettingInt(AppSettingsKey key) => key switch
        {
            AppSettingsKey.ImageQuality => Task.FromResult((int?)75),
            AppSettingsKey.CropImageMaxScaledHeight => Task.FromResult((int?)600),
            AppSettingsKey.CropImageMaxScaledWidth => Task.FromResult((int?)800),
            AppSettingsKey.ThumbnailSize => Task.FromResult((int?)150),            
            _ => throw new Exception($"The stub does not know what value to return for '{key}'")
        };
        public Task UpdateAppSetting(AppSettingsKey key, string value) => throw new NotImplementedException();
    }
}
