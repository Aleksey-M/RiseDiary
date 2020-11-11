using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.WebUI.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarItemsController : ControllerBase
    {
        private readonly ICalendarService _calendarService;
        public CalendarItemsController(ICalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        [HttpGet, Route("api/v1.0/calendar/{year}")]
        [ProducesResponseType(typeof(CalendarDateDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CalendarDateDto>>> GetCalendarDates([FromRoute] int year, [FromQuery] IEnumerable<Guid>? themeId, [FromQuery] bool? combinedThemes)
        {
            bool ct = combinedThemes ?? false;
            var themesIds = themeId ?? Enumerable.Empty<Guid>();

            var items = await _calendarService.GetCalendarItems(year, themesIds, ct);

            return items.Select(i => new CalendarDateDto
            {
                Id = i.Id.ToString(),
                Name = i?.Name ?? "",
                StartDate = i!.StartDate,
                EndDate = i.EndDate
            }).ToList();
        }

        [HttpGet, Route("api/v1.0/calendar/years")]
        [ProducesResponseType(typeof(Int32), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<int>>> GetCalendarYears([FromQuery] IEnumerable<Guid>? themeId, [FromQuery] bool? combinedThemes)
        {
            bool ct = combinedThemes ?? false;
            var themesIds = themeId ?? Enumerable.Empty<Guid>();

            return await _calendarService.GetYears(themesIds, ct);
        }
    }
}
