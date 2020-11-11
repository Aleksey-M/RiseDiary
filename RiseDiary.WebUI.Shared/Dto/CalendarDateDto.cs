using System;

namespace RiseDiary.WebUI.Shared.Dto
{
    public sealed class CalendarDateDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
