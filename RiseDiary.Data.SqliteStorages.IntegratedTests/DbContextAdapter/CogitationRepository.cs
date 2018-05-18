using RiseDiary.Data.SqliteStorages.IntegratedTests.Repositories;
using RiseDiary.Data.SqliteStorages.IntegratedTests.TestDomain;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests.DbContextAdapter
{
    public class CogitationRepository : ICogitationRepository
    {
        private readonly DiaryDbContext _context;
        public CogitationRepository(DiaryDbContext context)
        {
            _context = context;
        }

        public Task<int> AddCogitation(Cogitation cogitation)
        {
            var c = new Model.Cogitation { Id = cogitation.CogitationId, Date = cogitation.CogitationDate, RecordId = cogitation.RecordId, Text = cogitation.Text };
            return _context.AddCogitation(c);
        }

        public Task DeleteCogitation(int cogitationId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Cogitation>> FetchAllCogitationsOfRecord(int recordId)
        {
            throw new NotImplementedException();
        }

        public Task<Cogitation> FetchCogitationById(int cogitationId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetCogitationsCount(int recordId)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateCogitation(Cogitation cogitation)
        {
            throw new NotImplementedException();
        }
    }
}
