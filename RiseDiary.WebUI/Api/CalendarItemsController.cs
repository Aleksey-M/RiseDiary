using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.WebUI.Shared.Dto;

namespace RiseDiary.WebUI.Api
{
    [ApiController]
    [Authorize]
    public sealed class CalendarItemsController : ControllerBase
    {
        private readonly ICalendarService _calendarService;

        public CalendarItemsController(ICalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        [HttpGet, Route("api/v1.0/calendar/{year}")]
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

        [HttpGet, Route("api/v1.0/calendar/years")]
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
}
