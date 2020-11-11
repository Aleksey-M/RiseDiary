using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Model
{
    public record DateListItem(Guid Id, DateTime Date, DateTime TransferredDate, string Name, string Text, string Themes);

    public interface IDatesService
    {
        Task<List<DateListItem>> GetAllDates(DateTime today);
        Task<List<DateListItem>> GetDatesFromRange(DateTime tuday, bool withEmptyDates);
    }
}
