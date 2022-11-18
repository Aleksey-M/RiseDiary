using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared.Dto;

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

    [HttpGet("all")]
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

    [HttpGet("range")]
    public async Task<ActionResult<List<DateListItemDto>>> GetDatesRange(
        CancellationToken cancellationToken, [FromQuery] bool withEmptyDates = true)
    {
        var records = await _datesService.GetDatesFromRange(
            DateOnly.FromDateTime(DateTime.UtcNow), withEmptyDates, cancellationToken);

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
