using Microsoft.EntityFrameworkCore;
using RiseDiary.Data;
using RiseDiary.Model;

namespace RiseDiary.WebAPI.Services;

public sealed class CalendarService : ICalendarService
{
    private readonly DiaryDbContext _context;
    private readonly ILogger<CalendarService> _logger;

    public CalendarService(DiaryDbContext context, ILogger<CalendarService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<CalendarItem>> GetCalendarItems(
        int year,
        IEnumerable<Guid> themesId,
        bool combineThemes,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Чтение списка записей {year} года из БД для отображения на календаре",
            year);

        var yearRecords = await _context.Records
            .Where(x => x.Date.Year == year)
            .Select(x => new { x.Id, x.Name, x.Date, Themes = x.ThemesRefs.Select(t => t.ThemeId) })
            .ToListAsync(cancellationToken: cancellationToken);

        if (themesId is null || !themesId.Any())
        {
            return yearRecords
                .Select(r => new CalendarItem(
                    r.Id,
                    string.IsNullOrWhiteSpace(r.Name) ? "[ПУСТО]" : r.Name,
                    r.Date, r.Date))
                .OrderBy(r => r.StartDate)
                .ToList();
        }
        else
        {
            Func<IEnumerable<Guid>, Func<Guid, bool>, bool> filterFunc = combineThemes
                ? Enumerable.Any
                : Enumerable.All;

            return yearRecords
                .Where(r => filterFunc(themesId, t => r.Themes.Contains(t)))
                .Select(r => new CalendarItem(
                    r.Id,
                    string.IsNullOrWhiteSpace(r.Name) ? "[ПУСТО]" : r.Name,
                    r.Date, r.Date))
                .OrderBy(r => r.StartDate)
                .ToList();
        }
    }

    public async Task<List<int>> GetYears(IEnumerable<Guid> themesId, bool combineThemes, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Чтение списка лет для календаря");

        var calendarYears = await _context.Records
            .Select(r => new { r.Date, Themes = r.ThemesRefs.Select(t => t.ThemeId) })
            .ToListAsync(cancellationToken);

        if (themesId is null || !themesId.Any())
        {
            return calendarYears
                .Select(x => x.Date.Year)
                .Distinct()
                .Order()
                .ToList();
        }
        else
        {
            Func<IEnumerable<Guid>, Func<Guid, bool>, bool> filterFunc = combineThemes
                ? Enumerable.Any
                : Enumerable.All;

            return calendarYears
                .Where(r => filterFunc(themesId, t => r.Themes.Contains(t)))
                .Select(x => x.Date.Year)
                .Distinct()
                .Order()
                .ToList();
        }
    }
}
