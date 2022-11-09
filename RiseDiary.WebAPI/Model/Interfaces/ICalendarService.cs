namespace RiseDiary.Model;

public record CalendarItem(Guid Id, string Name, DateOnly StartDate, DateOnly EndDate);

public interface ICalendarService
{
    Task<List<int>> GetYears(IEnumerable<Guid> themesId,
        bool combineThemes, CancellationToken cancellationToken = default);

    Task<List<CalendarItem>> GetCalendarItems(int year,
        IEnumerable<Guid> themesId, bool combineThemes, CancellationToken cancellationToken = default);
}
