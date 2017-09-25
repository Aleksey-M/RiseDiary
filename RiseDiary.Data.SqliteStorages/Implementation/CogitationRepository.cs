using System.Collections.Generic;
using System.Threading.Tasks;
using RiseDiary.Domain.Model;
using RiseDiary.Domain.Repositories;

namespace RiseDiary.Data.SqliteStorages
{
    public class CogitationRepository : ICogitationRepository
    {
        public async Task DeleteCogitation(int cogitationId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Cogitation> FetchCogitationById(int cogitationId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<Cogitation>> FetchAllCogitationsOfRecord(int recordId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<int> AddCogitation(Cogitation cogitation)
        {
            throw new System.NotImplementedException();
        }

        public async Task<int> UpdateCogitation(Cogitation cogitation)
        {
            throw new System.NotImplementedException();
        }

        public async Task<int> GetCogitationsCount(int recordId)
        {
            throw new System.NotImplementedException();
        }
    }
}
