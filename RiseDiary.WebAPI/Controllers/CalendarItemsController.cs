using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.WebAPI.Shared.Dto;

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

    [HttpGet, Route("{year}")]
    [ProducesResponseType(typeof(CalendarDateDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CalendarDateDto>>> GetCalendarDates([FromRoute] int year,
        [FromQuery] IEnumerable<Guid>? themeId, [FromQuery] bool? combinedThemes, CancellationToken cancellationToken)
    {
        bool ct = combinedThemes ?? false;
        var themesIds = themeId ?? Enumerable.Empty<Guid>();

        try
        {
            var items = await _calendarService.GetCalendarItems(year, themesIds, ct, cancellationToken);

            return items.Select(i => new CalendarDateDto
            {
                Id = i.Id.ToString(),
                Name = i?.Name ?? "",
                StartDate = i!.StartDate,
                EndDate = i.EndDate
            }).ToList();
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
    }

    [HttpGet, Route("years")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<int>>> GetCalendarYears([FromQuery] IEnumerable<Guid>? themeId,
        [FromQuery] bool? combinedThemes, CancellationToken cancellationToken)
    {
        bool ct = combinedThemes ?? false;
        var themesIds = themeId ?? Enumerable.Empty<Guid>();

        try
        {
            return await _calendarService.GetYears(themesIds, ct, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
    }
}
