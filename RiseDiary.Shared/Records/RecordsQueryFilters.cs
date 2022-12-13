namespace RiseDiary.Shared.Records;

public sealed class RecordsSearchFilters
{
    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }

    public string? Name { get; set; }

    public bool CombinedThemes { get; set; }

    public string? Themes { get; set; }

    public bool Expanded { get; set; }
}
