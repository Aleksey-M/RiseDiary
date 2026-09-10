using Microsoft.EntityFrameworkCore;
using RiseDiary.Data;
using RiseDiary.WebAPI.Settings.Model;

namespace RiseDiary.WebAPI.Settings.Storage;

public class ImportantDaysSettingsStorage : ISettingsStorage<ImportantDaysSettings>
{
    private const int DefaultDaysCount = 10;
    private readonly SettingsKey[] keys = [SettingsKey.ImportantDaysScopeId, SettingsKey.ImportantDaysDisplayRange];
    private readonly DiaryDbContext _context;

    public ImportantDaysSettingsStorage(DiaryDbContext context)
    {
        _context = context;
    }

    public async Task<ImportantDaysSettings> Read(CancellationToken cancellationToken)
    {
        var settings = await _context.AppSettings
            .AsNoTracking()
            .Where(x => keys.Contains(x.Key))
            .ToDictionaryAsync(x => x.Key, cancellationToken: cancellationToken);

        return new ImportantDaysSettings(
            settings.TryGetValue(SettingsKey.ImportantDaysScopeId, out var sid) && Guid.TryParse(sid.Value, out var sidValue)
                ? sidValue
                : null,
            settings.TryGetValue(SettingsKey.ImportantDaysDisplayRange, out var daysRange) && int.TryParse(daysRange.Value, out var daysRangeValue)
                ? daysRangeValue
                : DefaultDaysCount,
            settings.Values.Select(x => x.ModifiedDate).DefaultIfEmpty().Max());
    }

    public async Task Write(ImportantDaysSettings settings)
    {
        var sList = await _context.AppSettings
            .Where(x => keys.Contains(x.Key))
            .ToArrayAsync();

        var sid = sList.FirstOrDefault(x => x.Key == SettingsKey.ImportantDaysScopeId);
        if (sid == null)
        {
            sid = new SettingEntity
            {
                Key = SettingsKey.ImportantDaysScopeId
            };
            _context.AppSettings.Add(sid);
        }

        sid.Value = settings.ImportantDaysScopeId?.ToString() ?? string.Empty;
        sid.ModifiedDate = DateTime.UtcNow;

        var dr = sList.FirstOrDefault(x => x.Key == SettingsKey.ImportantDaysDisplayRange);
        if (dr == null)
        {
            dr = new SettingEntity
            {
                Key = SettingsKey.ImportantDaysDisplayRange
            };
            _context.AppSettings.Add(dr);
        }

        dr.Value = settings.ImportantDaysDisplayRange.ToString();
        dr.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}
