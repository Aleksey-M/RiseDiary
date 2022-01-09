using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Model
{
    public record DateListItem(Guid Id, DateOnly Date, DateOnly TransferredDate, string Name, string Text, string Themes);

    public interface IDatesService
    {
        Task<List<DateListItem>> GetAllDates(DateOnly today);

        Task<List<DateListItem>> GetDatesFromRange(DateOnly tuday, bool withEmptyDates);
    }
}
