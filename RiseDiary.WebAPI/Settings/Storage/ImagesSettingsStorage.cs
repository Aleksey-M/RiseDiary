using Microsoft.EntityFrameworkCore;
using RiseDiary.Data;
using RiseDiary.WebAPI.Settings.Model;

namespace RiseDiary.WebAPI.Settings.Storage;

public sealed class ImagesSettingsStorage : ISettingsStorage<ImagesSettings>
{
    private const int DefaultImageQuality = 80;
    private const int DefaultThumbnailSize = 140;
    private readonly SettingsKey[] keys = [SettingsKey.ImageQuality, SettingsKey.ThumbnailSize];
    private readonly DiaryDbContext _context;

    public ImagesSettingsStorage(DiaryDbContext context)
    {
        _context = context;
    }

    public async Task<ImagesSettings> Read(CancellationToken cancellationToken)
    {
        var settings = await _context.AppSettings
            .AsNoTracking()
            .Where(x => keys.Contains(x.Key))
            .ToDictionaryAsync(x => x.Key, cancellationToken: cancellationToken);

        return new ImagesSettings(
            settings.TryGetValue(SettingsKey.ImageQuality, out var iq) && int.TryParse(iq.Value, out var iqValue)
                ? iqValue
                : DefaultImageQuality,
            settings.TryGetValue(SettingsKey.ThumbnailSize, out var ts) && int.TryParse(ts.Value, out var tsValue)
                ? tsValue
                : DefaultThumbnailSize,
            settings.Values.Select(x => x.ModifiedDate).DefaultIfEmpty().Max());
    }

    public async Task Write(ImagesSettings settings)
    {
        var sList = await _context.AppSettings
            .Where(x => keys.Contains(x.Key))
            .ToArrayAsync();

        var iq = sList.FirstOrDefault(x => x.Key == SettingsKey.ImageQuality);
        if (iq == null)
        {
            iq = new SettingEntity
            {
                Key = SettingsKey.ImageQuality
            };
            _context.AppSettings.Add(iq);
        }

        iq.Value = settings.ImageQuality.ToString();
        iq.ModifiedDate = DateTime.UtcNow;

        var ts = sList.FirstOrDefault(x => x.Key == SettingsKey.ThumbnailSize);
        if (ts == null)
        {
            ts = new SettingEntity
            {
                Key = SettingsKey.ThumbnailSize
            };
            _context.AppSettings.Add(ts);
        }

        ts.Value = settings.ThumbnailSize.ToString();
        ts.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}
