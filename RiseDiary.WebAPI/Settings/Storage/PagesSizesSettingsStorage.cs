using Microsoft.EntityFrameworkCore;
using RiseDiary.Data;
using RiseDiary.WebAPI.Settings.Model;

namespace RiseDiary.WebAPI.Settings.Storage;

public class PagesSizesSettingsStorage : ISettingsStorage<PagesSizesSettings>
{
    private const int DefaultRecordsPageSize = 50;
    private const int DefaultImagesPageSize = 20;
    private const int DefaultAvailableImagesPageSize = 20;
    private readonly SettingsKey[] keys = [SettingsKey.RecordsPageSize, SettingsKey.ImagesPageSize, SettingsKey.AvailableImagesPageSize];
    private readonly DiaryDbContext _context;

    public PagesSizesSettingsStorage(DiaryDbContext context)
    {
        _context = context;
    }

    public async Task<PagesSizesSettings> Read(CancellationToken cancellationToken)
    {
        var settings = await _context.AppSettings
            .AsNoTracking()
            .Where(x => keys.Contains(x.Key))
            .ToDictionaryAsync(x => x.Key, cancellationToken: cancellationToken);

        return new PagesSizesSettings(
            settings.TryGetValue(SettingsKey.ImagesPageSize, out var ps1) && int.TryParse(ps1.Value, out var ps1Value)
                ? ps1Value
                : DefaultImagesPageSize,
            settings.TryGetValue(SettingsKey.RecordsPageSize, out var ps2) && int.TryParse(ps2.Value, out var ps2Value)
                ? ps2Value
                : DefaultRecordsPageSize,
            settings.TryGetValue(SettingsKey.AvailableImagesPageSize, out var ps3) && int.TryParse(ps3.Value, out var ps3Value)
                ? ps3Value
                : DefaultAvailableImagesPageSize,
            settings.Values.Select(x => x.ModifiedDate).DefaultIfEmpty().Max());
    }

    public async Task Write(PagesSizesSettings settings)
    {
        var sList = await _context.AppSettings
            .Where(x => keys.Contains(x.Key))
            .ToArrayAsync();

        var ps1 = sList.FirstOrDefault(x => x.Key == SettingsKey.ImagesPageSize);
        if (ps1 == null)
        {
            ps1 = new SettingEntity
            {
                Key = SettingsKey.ImagesPageSize
            };
            _context.AppSettings.Add(ps1);
        }

        ps1.Value = settings.ImagesPageSize.ToString();
        ps1.ModifiedDate = DateTime.UtcNow;

        var ps2 = sList.FirstOrDefault(x => x.Key == SettingsKey.RecordsPageSize);
        if (ps2 == null)
        {
            ps2 = new SettingEntity
            {
                Key = SettingsKey.RecordsPageSize
            };
            _context.AppSettings.Add(ps2);
        }

        ps2.Value = settings.RecordsPageSize.ToString();
        ps2.ModifiedDate = DateTime.UtcNow;

        var ps3 = sList.FirstOrDefault(x => x.Key == SettingsKey.AvailableImagesPageSize);
        if (ps3 == null)
        {
            ps3 = new SettingEntity
            {
                Key = SettingsKey.AvailableImagesPageSize
            };
            _context.AppSettings.Add(ps3);
        }

        ps3.Value = settings.AvailableImagesPageSize.ToString();
        ps3.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}
