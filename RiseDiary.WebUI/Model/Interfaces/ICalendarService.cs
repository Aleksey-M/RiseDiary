using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Model
{
    public record CalendarItem(Guid Id, string Name, DateTime StartDate, DateTime EndDate);

    public interface ICalendarService
    {
        Task<List<int>> GetYears(IEnumerable<Guid> themesId, bool combineThemes);

        Task<List<CalendarItem>> GetCalendarItems(int year, IEnumerable<Guid> themesId, bool combineThemes);
    }
}
