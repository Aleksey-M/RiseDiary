using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared.Calendar;

namespace RiseDiary.Api;

[ApiController]
public sealed class CalendarItemsController : ControllerBase
{
    private readonly ICalendarService _calendarService;

    public CalendarItemsController(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    [HttpGet("api/records/calendar")]
    public async Task<ActionResult<CalendarDto>> GetCalendar(
        [FromQuery] int? year,
        [FromQuery] string? themes,
        [FromQuery] bool? combinedThemes,
        CancellationToken cancellationToken)
    {
        var _combined = combinedThemes ?? false;
        var _year = year.HasValue && year.Value != default ? year.Value : DateTime.UtcNow.Year;
        var themesIds = new List<Guid>();

        if (!string.IsNullOrWhiteSpace(themes))
        {
            foreach (var strId in themes.Split(","))
            {
                if (Guid.TryParse(strId, out var id))
                {
                    themesIds.Add(id);
                }
            }
        }

        var data = new CalendarDto
        {
            Years = await _calendarService.GetYears(themesIds, _combined, cancellationToken)
        };

        if (data.Years.Any() && !data.Years.Contains(_year))
        {
            _year = data.Years.Last();
        }

        var dates = await _calendarService.GetCalendarItems(_year, themesIds, _combined, cancellationToken);
        data.Dates = dates.Select(x => x.ToDto()).ToList();

        data.CurrentYear = _year;

        return data;
    }
}


internal static class DtoExtensions
{
    public static CalendarDateDto ToDto(this CalendarItem calendarItem) => new()
    {
        Id = calendarItem.Id.ToString(),
        Name = calendarItem?.Name ?? "",
        StartDate = calendarItem!.StartDate,
        EndDate = calendarItem.EndDate
    };
}
