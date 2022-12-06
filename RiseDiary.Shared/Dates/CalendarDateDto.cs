namespace RiseDiary.Shared.Dates;

public sealed class CalendarDateDto
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }
}
