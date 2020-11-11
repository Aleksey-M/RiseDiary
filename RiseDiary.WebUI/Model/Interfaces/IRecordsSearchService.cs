using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Model
{
    public interface IRecordsSearchService
    {
        Task<List<DiaryRecord>> GetRecordsList(RecordsFilter filter);
        Task<int> GetRecordsCount(RecordsFilter filter);
    }
}
