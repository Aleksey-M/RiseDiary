using Microsoft.EntityFrameworkCore;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.Model.Services
{
    internal sealed class RecordsThemesService : IRecordsThemesService
    {
        private readonly DiaryDbContext _context;

        public RecordsThemesService(DiaryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddRecordTheme(Guid recordId, IEnumerable<Guid> themesIds)
        {
            if (!themesIds.Any()) return;

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

            foreach (var themeId in themesIds)
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

        public async Task<List<DiaryRecordTheme>> GetRecordThemes(Guid recordId) => await _context.RecordThemes
            .AsNoTracking()
            .Include(x => x.Theme)
            .Where(x => x.RecordId == recordId)
            .ToListAsync()
            .ConfigureAwait(false);

        public async Task RemoveRecordTheme(Guid recordId, IEnumerable<Guid> themesIds)
        {
            if (!themesIds.Any()) return;

            var record = await _context.Records
                .Include(r => r.ThemesRefs)
                .SingleOrDefaultAsync(r => r.Id == recordId)
                .ConfigureAwait(false);

            if (record == null) return;

            foreach (var tId in themesIds)
            {
                var recTheme = record.ThemesRefs.SingleOrDefault(rt => rt.ThemeId == tId);
                if (recTheme != null) _context.Entry(recTheme).State = EntityState.Deleted;
            }

            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
