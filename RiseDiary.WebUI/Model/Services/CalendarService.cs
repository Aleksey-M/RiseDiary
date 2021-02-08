using Microsoft.EntityFrameworkCore;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.Model.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly DiaryDbContext _context;
        private readonly IAppSettingsService _appSettingsService;

        public CalendarService(DiaryDbContext context, IAppSettingsService appSettingsService)
        {
            _context = context;
            _appSettingsService = appSettingsService;
        }

        public async Task<List<CalendarItem>> GetCalendarItems(int year, IEnumerable<Guid> themesId, bool combineThemes)
        {
            var placeholder = _appSettingsService.GetHostAndPortPlaceholder();
            var currentHostAndPort = await _appSettingsService.GetHostAndPort();

            if (themesId is null || !themesId.Any())
            {
                var records = await _context.Records
                    .AsNoTracking()
                    .Select(r => new CalendarItem(
                        r.Id,
                        string.IsNullOrWhiteSpace(r.Name) ? "[ПУСТО]" : r.Name.Replace(currentHostAndPort, placeholder, StringComparison.OrdinalIgnoreCase),
                        r.Date, r.Date))
                    .ToListAsync()
                    .ConfigureAwait(false);

                records = records
                    .Where(r => r.StartDate.Year == year)
                    .OrderBy(r => r.StartDate)
                    .ToList();

                return records;
            }
            else
            {
                Func<IEnumerable<Guid>, Func<Guid, bool>, bool> filterFunc = combineThemes ? Enumerable.Any : Enumerable.All;

                var items = await _context.Records
                    .AsNoTracking()
                    .Select(r => new { r.Id, r.Date, r.Name, Themes = r.ThemesRefs.Select(t => t.ThemeId) })
                    .AsSingleQuery()
                    .ToListAsync()
                    .ConfigureAwait(false);

                var records = items
                    .Where(i => i.Date.Year == year)
                    .Where(r => filterFunc(themesId, t => r.Themes.Contains(t)))
                    .Select(r => new CalendarItem(
                        r.Id,
                        string.IsNullOrWhiteSpace(r.Name) ? "[ПУСТО]" : r.Name.Replace(currentHostAndPort, placeholder, StringComparison.OrdinalIgnoreCase),
                        r.Date, r.Date))
                    .OrderBy(r => r.StartDate)
                    .ToList();

                return records;
            }
        }

        public async Task<List<int>> GetYears(IEnumerable<Guid> themesId, bool combineThemes)
        {
            if (themesId is null || !themesId.Any())
            {
                var years = await _context.Records
                    .AsNoTracking()
                    .Select(r => r.Date.Year)
                    .Distinct()
                    .OrderBy(y => y)
                    .ToListAsync()
                    .ConfigureAwait(false);

                return years;
            }
            else
            {
                Func<IEnumerable<Guid>, Func<Guid, bool>, bool> filterFunc = combineThemes ? Enumerable.Any : Enumerable.All;

                var items = await _context.Records
                    .AsNoTracking()
                    .Select(r => new { r.Date, Themes = r.ThemesRefs.Select(t => t.ThemeId) })
                    .ToListAsync()
                    .ConfigureAwait(false);

                var years = items
                    .Where(r => filterFunc(themesId, t => r.Themes.Contains(t)))
                    .Select(r => r.Date.Year)
                    .Distinct()
                    .OrderBy(y => y)
                    .ToList();

                return years;
            }
        }
    }
}
