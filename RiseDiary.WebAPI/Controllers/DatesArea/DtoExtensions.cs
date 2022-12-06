using RiseDiary.Model;
using RiseDiary.Shared.Calendar;

namespace RiseDiary.WebAPI.Controllers.DatesArea;

internal static class DtoExtensions
{
    public static DateListItemDto ToDto(this DateListItem dateListItem) => new()
    {
        Id = dateListItem.Id,
        Date = dateListItem.Date,
        TransferredDate = dateListItem.TransferredDate,
        TransferredDateStr = dateListItem.TransferredDate.ToString("yyyy.MM.dd ddd"),
        Name = dateListItem.Name,
        Text = dateListItem.Text,
        Themes = dateListItem.Themes
    };

    public static CalendarDateDto ToCalendarDto(this DateListItem dateListItem) => new CalendarDateDto
    {
        Id = dateListItem.Id.ToString(),
        Name = $"{dateListItem.Themes} : {dateListItem.Name}",
        StartDate = dateListItem.TransferredDate,
        EndDate = dateListItem.TransferredDate
    };
}
