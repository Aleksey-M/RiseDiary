using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared.Calendar;

namespace RiseDiary.WebAPI.Controllers.DatesArea;

[ApiController]
[Route("api/dates")]
public sealed class DatesController : ControllerBase
{
    private readonly IDatesService _datesService;

    public DatesController(IDatesService datesService)
    {
        _datesService = datesService;
    }

    [HttpGet("fulllist")]
    public async Task<ActionResult<List<DateListItemDto>>> GetAllDates(CancellationToken cancellationToken)
    {
        var records = await _datesService.GetAllDates(DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

        return records.Select(x => x.ToDto()).ToList();
    }

    [HttpGet("nearest")]
    public async Task<ActionResult<List<DateListItemDto>>> GetDatesRange(CancellationToken cancellationToken)
    {
        var records = await _datesService.GetDatesFromRange(
            DateOnly.FromDateTime(DateTime.UtcNow), true, cancellationToken);

        return records.Select(x => x.ToDto()).ToList();
    }

    [HttpGet("calendar")]
    public async Task<ActionResult<List<CalendarDateDto>>> GetCalendarDates(CancellationToken cancellationToken)
    {
        var records = await _datesService.GetAllDates(DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

        return records.Select(x => x.ToCalendarDto()).ToList();
    }
}