namespace RiseDiary.Shared.Calendar;

public sealed class CalendarDto
{
    public List<CalendarDateDto> Dates { get; set; } = new();

    public List<int> Years { get; set; } = new();

    public int CurrentYear { get; set; }
}