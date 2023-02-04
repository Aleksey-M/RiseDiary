namespace RiseDiary.Shared.Calendar;

public sealed class CalendarDateDto
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }
}

public static class CalendarDateDtoExtensions
{
    public static void SetBaseUri(this CalendarDateDto dto, string baseUri)
    {
        dto.Name = InternalLinksHelper.SetBaseUri(dto.Name!.Trim(), baseUri);
    }
}