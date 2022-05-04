using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.WebUI.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Api
{
    [ApiController]
    [Authorize]
    public sealed class DatesController : ControllerBase
    {
        private readonly IDatesService _datesService;

        public DatesController(IDatesService datesService)
        {
            _datesService = datesService;
        }

        [HttpGet, Route("api/v1.0/dates/all")]
        public async Task<ActionResult<List<DateListItemDto>>> GetAllDates()
        {
            var records = await _datesService.GetAllDates(DateOnly.FromDateTime(DateTime.UtcNow));

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

        [HttpGet, Route("api/v1.0/dates/range")]
        public async Task<ActionResult<List<DateListItemDto>>> GetDatesRange([FromQuery] bool withEmptyDates = true)
        {
            var records = await _datesService.GetDatesFromRange(DateOnly.FromDateTime(DateTime.UtcNow), withEmptyDates);

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

        [HttpGet, Route("api/v1.0/dates/calendar")]
        public async Task<ActionResult<List<CalendarDateDto>>> GetCalendarDates()
        {
            var records = await _datesService.GetAllDates(DateOnly.FromDateTime(DateTime.UtcNow));

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

    }
}
