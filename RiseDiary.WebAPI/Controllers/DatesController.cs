﻿using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.WebAPI.Shared.Dto;

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

    [HttpGet, Route("all")]
    public async Task<ActionResult<List<DateListItemDto>>> GetAllDates(CancellationToken cancellationToken)
    {
        try
        {
            var records = await _datesService.GetAllDates(DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

            var dto = records.Select(d => new DateListItemDto
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

            return Ok(dto);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
    }

    [HttpGet, Route("range")]
    public async Task<ActionResult<List<DateListItemDto>>> GetDatesRange(
        CancellationToken cancellationToken, [FromQuery] bool withEmptyDates = true)
    {
        try
        {
            var records = await _datesService.GetDatesFromRange(
                DateOnly.FromDateTime(DateTime.UtcNow), withEmptyDates, cancellationToken);

            var dto = records.Select(d => new DateListItemDto
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

            return Ok(dto);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
    }

    [HttpGet, Route("calendar")]
    public async Task<ActionResult<List<CalendarDateDto>>> GetCalendarDates(CancellationToken cancellationToken)
    {
        try
        {
            var records = await _datesService.GetAllDates(DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

            var dto = records.Select(d => new CalendarDateDto
            {
                Id = d.Id.ToString(),
                Name = d.Themes + ": " + d.Name,
                StartDate = d.TransferredDate,
                EndDate = d.TransferredDate
            })
                .ToList();

            return Ok(dto);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
    }

}
