using System;

namespace RiseDiary.WebUI
{
    public sealed class BrowserTimeOffsetService
    {
        public int? Offset { get; set; }

        public string ToLocalString(DateTime utcDate) => utcDate
            .AddMinutes(Offset.HasValue ? -Offset.Value : 0)
            .ToString("yyyy.MM.dd HH:mm:ss");
    }
}
