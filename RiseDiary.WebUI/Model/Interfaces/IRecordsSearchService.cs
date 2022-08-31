using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RiseDiary.Model
{
    public interface IRecordsSearchService
    {
        Task<List<DiaryRecord>> GetRecordsList(RecordsFilter filter, CancellationToken cancellationToken = default);

        Task<int> GetRecordsCount(RecordsFilter filter, CancellationToken cancellationToken = default);

        Task<List<DiaryRecord>> GetThisDayRecords(int month, int day, CancellationToken cancellationToken = default);
    }
}
