using Microsoft.EntityFrameworkCore;
using RiseDiary.Data;

namespace RiseDiary.Model.Services;

internal sealed class RecordsThemesService : IRecordsThemesService
{
    private readonly DiaryDbContext _context;

    public RecordsThemesService(DiaryDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task UpdateRecordThemes(Guid recordId, IEnumerable<Guid> themesIds)
    {
        var record = await _context.Records
            .Include(r => r.ThemesRefs)
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(r => r.Id == recordId)
            .ConfigureAwait(false);

        if (record == null) throw new ArgumentException($"Record with id '{recordId}' does not exists");
        if (record.Deleted) throw new ArgumentException($"Record with id '{recordId}' is deleted");

        var existedThemesIds = await _context.Themes
            .AsNoTracking()
            .Select(t => t.Id)
            .ToListAsync()
            .ConfigureAwait(false);

        var notExisted = themesIds.Except(existedThemesIds).ToList();
        if (notExisted.Count > 0)
        {
            throw new ArgumentException($@"Theme(s) with id: {string.Concat(notExisted.Select(id => id.ToString()), ',')} does not exist");
        }

        // removing
        var deletedThemes = record.ThemesRefs
           .Where(x => !themesIds.Contains(x.ThemeId))
           .ToList();

        if (deletedThemes.Any())
        {
            deletedThemes.ForEach(x => _context.Entry(x).State = EntityState.Deleted);
        }

        // adding
        var newThemes = themesIds
            .Where(x =>
                !record.ThemesRefs.Any(y => y.ThemeId == x)
                || record.ThemesRefs.Any(y => y.ThemeId == x && y.Deleted))
            .ToList();

        foreach (var themeId in newThemes)
        {
            var deletedTheme = record.ThemesRefs.SingleOrDefault(rt => rt.ThemeId == themeId);
            if (deletedTheme != null)
            {
                deletedTheme.Deleted = false;
            }
            else
            {
                await _context.RecordThemes.AddAsync(new DiaryRecordTheme { ThemeId = themeId, RecordId = record.Id });
            }
        }

        await _context.SaveChangesAsync().ConfigureAwait(false);
    }
}
