namespace RiseDiary.Shared.Calendar;

public sealed class CalendarDateDto
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }
}


public sealed class CalendarDto
{
    public List<CalendarDateDto> Dates { get; set; } = new();

    public List<int> Years { get; set; } = new();

    public int CurrentYear { get; set; }
}