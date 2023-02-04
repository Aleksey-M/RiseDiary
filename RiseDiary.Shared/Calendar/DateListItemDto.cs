namespace RiseDiary.Shared.Calendar;

public sealed class DateListItemDto
{
    public Guid Id { get; set; }

    public DateOnly Date { get; set; }

    public DateOnly TransferredDate { get; set; }

    public string? TransferredDateStr { get; set; }

    public string? Name { get; set; }

    public string? Text { get; set; }

    public string? Themes { get; set; }
}

public static class DateListItemDtoExtensions
{
    public static void SetBaseUri(this DateListItemDto dto, string baseUri)
    {
        if (dto.Name != null)
        {
            dto.Name = InternalLinksHelper.SetBaseUri(dto.Name.Trim(), baseUri);
        }

        if (dto.Text != null)
        {
            dto.Text = InternalLinksHelper.SetBaseUri(dto.Text.Trim(), baseUri);
        }
    }
}
