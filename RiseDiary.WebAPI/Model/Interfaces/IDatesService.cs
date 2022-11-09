namespace RiseDiary.Model;

public record DateListItem(Guid Id, DateOnly Date, DateOnly TransferredDate, string Name, string Text, string Themes);

public interface IDatesService
{
    Task<List<DateListItem>> GetAllDates(DateOnly today, CancellationToken cancellationToken = default);

    Task<List<DateListItem>> GetDatesFromRange(DateOnly tuday,
        bool withEmptyDates, CancellationToken cancellationToken = default);
}