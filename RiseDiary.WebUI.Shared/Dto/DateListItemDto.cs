using System;

namespace RiseDiary.WebUI.Shared.Dto
{
    public sealed class DateListItemDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime TransferredDate { get; set; }
        public string? TransferredDateStr { get; set; }
        public string? Name { get; set; }
        public string? Text { get; set; }
        public string? Themes { get; set; }
    }
}
