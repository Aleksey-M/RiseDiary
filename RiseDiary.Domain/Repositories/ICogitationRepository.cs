using RiseDiary.Domain.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Domain.Repositories
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
