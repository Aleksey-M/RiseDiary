using Microsoft.EntityFrameworkCore;
using RiseDiary.Data;
using RiseDiary.WebAPI.Settings.Model;

namespace RiseDiary.WebAPI.Settings.Storage;

public class BookmarksSettingsStorage : ISettingsStorage<BookmarksSettings>
{
    private readonly DiaryDbContext _context;

    public BookmarksSettingsStorage(DiaryDbContext context)
    {
        _context = context;
    }

    public async Task<BookmarksSettings> Read(CancellationToken cancellationToken)
    {
        var setting = await _context.AppSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == SettingsKey.BookmarksRecordsList, cancellationToken: cancellationToken);

        return new BookmarksSettings(
            setting?.Value
                .Trim()
                .Split(';')
                .Select(x => (isGuid: Guid.TryParse(x, out var id), id))
                .Where(x => x.isGuid)
                .Select(x => x.id)
                .ToArray() ?? [],
            setting?.ModifiedDate ?? DateTime.MinValue);
    }

    public async Task Write(BookmarksSettings settings)
    {
        var settingValue = string.Join(';', settings.Records.Distinct());

        var setting = await _context.AppSettings
            .FirstOrDefaultAsync(s => s.Key == SettingsKey.BookmarksRecordsList);

        if (setting == null)
        {
            setting = new SettingEntity
            {
                Key = SettingsKey.BookmarksRecordsList
            };
            _context.AppSettings.Add(setting);
        }

        setting.Value = settingValue;
        setting.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}
