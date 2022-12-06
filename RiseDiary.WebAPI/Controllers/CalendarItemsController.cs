using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared.Calendar;

namespace RiseDiary.Api;

[ApiController]
[Route("api/calendar")]
public sealed class CalendarItemsController : ControllerBase
{
    private readonly ICalendarService _calendarService;

    public CalendarItemsController(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    [HttpGet("{year}")]
    public async Task<ActionResult<List<CalendarDateDto>>> GetCalendarDates([FromRoute] int year,
        [FromQuery] IEnumerable<Guid>? themeId, [FromQuery] bool? combinedThemes, CancellationToken cancellationToken)
    {
        bool ct = combinedThemes ?? false;
        var themesIds = themeId ?? Enumerable.Empty<Guid>();

        var items = await _calendarService.GetCalendarItems(year, themesIds, ct, cancellationToken);

        return items.Select(x => x.ToDto()).ToList();
    }

    [HttpGet("years")]
    public async Task<ActionResult<IEnumerable<int>>> GetCalendarYears([FromQuery] IEnumerable<Guid>? themeId,
        [FromQuery] bool? combinedThemes, CancellationToken cancellationToken)
    {
        bool ct = combinedThemes ?? false;
        var themesIds = themeId ?? Enumerable.Empty<Guid>();

        return await _calendarService.GetYears(themesIds, ct, cancellationToken);
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
