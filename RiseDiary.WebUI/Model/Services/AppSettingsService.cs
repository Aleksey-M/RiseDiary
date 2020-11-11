using Microsoft.EntityFrameworkCore;
using RiseDiary.Shared;
using RiseDiary.WebUI.Data;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace RiseDiary.Model.Services
{
    public class AppSettingsService : IAppSettingsService
    {
        private readonly DiaryDbContext _context;
        public AppSettingsService(DiaryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<(string? value, DateTime? modifiedDate)> GetAppSetting(AppSettingsKey key)
        {
            if (key == AppSettingsKey.Unknown) throw new ArgumentException("Unknown settings key");

            var keyStr = key.ToString();
            var setting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == keyStr).ConfigureAwait(false);
            return (setting?.Value, setting?.ModifiedDate);
        }

        public async Task<int?> GetAppSettingInt(AppSettingsKey key)
        {
            var (str, _) = await GetAppSetting(key).ConfigureAwait(false);
            return int.TryParse(str, NumberStyles.None, CultureInfo.CurrentCulture.NumberFormat, out int result) ? result : (int?)null;
        }

        public async Task UpdateAppSetting(AppSettingsKey key, string value)
        {
            var message = SettingsValidator.Validate(key, value);
            if (message != "") throw new ArgumentException(message);

            var keyStr = key.ToString();
            var appSetting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == keyStr).ConfigureAwait(false);

            if (appSetting == null)
            {
                await _context.AppSettings.AddAsync(new AppSetting
                {
                    Key = keyStr,
                    Value = value,
                    ModifiedDate = DateTime.Now
                });
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            else
            {
                if (appSetting.Value != value)
                {
                    appSetting.Value = value;
                    appSetting.ModifiedDate = DateTime.Now;
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
