namespace RiseDiary.Shared.Dto;

public sealed class RecordListItemDto
{
    public Guid RecordId { get; set; }

    public string DisplayedName { get; set; } = null!;

    public string DisplayedText { get; set; } = null!;

    public DateOnly Date { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public sealed class RecordsPageDto
{
    public PagesInfo PagesInfo { get; set; } = null!;

    public List<RecordListItemDto> Records { get; set; } = null!;
}

public sealed class RecordsDetailPageDto
{
    public PagesInfo PagesInfo { get; set; } = null!;

    public List<RecordDto> Records { get; set; } = null!;
}
