using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Model
{
    public interface IRecordsSearchTextService
    {
        Task<List<DiaryRecord>> GetRecordsList(RecordsTextFilter filter);
        Task<int> GetRecordsCount(string? searchText);
    }
}
