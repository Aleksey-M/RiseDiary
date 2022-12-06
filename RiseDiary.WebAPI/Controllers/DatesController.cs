using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared.Dates;

namespace RiseDiary.Api;

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

        return records.Select(d => new DateListItemDto
        {
            Id = d.Id,
            Date = d.Date,
            TransferredDate = d.TransferredDate,
            TransferredDateStr = d.TransferredDate.ToString("yyyy.MM.dd ddd", CultureInfo.CurrentCulture),
            Name = d.Name,
            Text = d.Text,
            Themes = d.Themes
        })
        .ToList();
    }

    [HttpGet("nearest")]
    public async Task<ActionResult<List<DateListItemDto>>> GetDatesRange(CancellationToken cancellationToken)
    {
        var records = await _datesService.GetDatesFromRange(
            DateOnly.FromDateTime(DateTime.UtcNow), true, cancellationToken);

        return records.Select(d => new DateListItemDto
        {
            Id = d.Id,
            Date = d.Date,
            TransferredDate = d.TransferredDate,
            TransferredDateStr = d.TransferredDate.ToString("yyyy.MM.dd ddd", CultureInfo.CurrentCulture),
            Name = d.Name,
            Text = d.Text,
            Themes = d.Themes
        })
        .ToList();
    }

    [HttpGet("calendar")]
    public async Task<ActionResult<List<CalendarDateDto>>> GetCalendarDates(CancellationToken cancellationToken)
    {
        var records = await _datesService.GetAllDates(DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

        return records.Select(d => new CalendarDateDto
        {
            Id = d.Id.ToString(),
            Name = d.Themes + ": " + d.Name,
            StartDate = d.TransferredDate,
            EndDate = d.TransferredDate
        })
        .ToList();
    }

}
