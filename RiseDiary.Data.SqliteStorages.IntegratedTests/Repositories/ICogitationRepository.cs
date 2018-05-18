using RiseDiary.Data.SqliteStorages.IntegratedTests.TestDomain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests.Repositories
{
    public interface ICogitationRepository
    {
        Task<int> AddCogitation(Cogitation cogitation);
        Task<Cogitation> FetchCogitationById(int cogitationId);
        Task<List<Cogitation>> FetchAllCogitationsOfRecord(int recordId);
        Task<int> UpdateCogitation(Cogitation cogitation);
        Task DeleteCogitation(int cogitationId);
        Task<int> GetCogitationsCount(int recordId);
    }
}
