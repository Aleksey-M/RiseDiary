using System.Collections.Generic;
using System.Threading.Tasks;
using RiseDiary.Domain.Model;
using RiseDiary.Domain.Repositories;

namespace RiseDiary.Data.SqliteStorages
{
    public class CogitationRepository : ICogitationRepository
    {
        public Task DeleteCogitation(int cogitationId)
        {
            throw new System.NotImplementedException();
        }

        public Task<Cogitation> FetchCogitationById(int cogitationId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Cogitation>> FetchAllCogitationsOfRecord(int recordId)
        {
            throw new System.NotImplementedException();
        }

        public Task<int> AddCogitation(Cogitation cogitation)
        {
            throw new System.NotImplementedException();
        }

        public Task<int> UpdateCogitation(Cogitation cogitation)
        {
            throw new System.NotImplementedException();
        }
    }
}
