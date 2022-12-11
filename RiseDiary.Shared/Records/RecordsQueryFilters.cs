namespace RiseDiary.Shared.Records;

public sealed class RecordsQueryFilters
{
    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }

    public string? Name { get; set; }

    public bool CombinedThemes { get; set; }

    public string? Themes { get; set; }

    public bool Expanded { get; set; }


    public Dictionary<string, object?> ToQueryParams() => new()
    {
        [nameof(From)] = From,
        [nameof(To)] = To,
        [nameof(Name)] = Name,
        [nameof(CombinedThemes)] = CombinedThemes,
        [nameof(Themes)] = Themes,
        [nameof(Expanded)] = Expanded
    };
}
