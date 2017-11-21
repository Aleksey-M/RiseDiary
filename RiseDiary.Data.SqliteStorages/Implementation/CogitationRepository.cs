using System.Collections.Generic;
using System.Threading.Tasks;
using RiseDiary.Domain.Model;
using RiseDiary.Domain.Repositories;
using Dapper;
using System.Linq;

namespace RiseDiary.Data.SqliteStorages
{
    public class CogitationRepository : ICogitationRepository
    {
        private readonly IDataBaseManager _manager;
        internal CogitationRepository(IDataBaseManager manager)
        {
            _manager = manager;
        }
        public async Task DeleteCogitation(int cogitationId)
        {
            using (var connection = await _manager.GetConnection())
            {
                await connection.ExecuteAsync("DELETE FROM Cogitations WHERE CogitationId = @cogitationId", new { cogitationId });
            }
        }

        public async Task<Cogitation> FetchCogitationById(int cogitationId)
        {
            using (var connection = await _manager.GetConnection())
            {
                return (await connection.QueryAsync<Cogitation>("SELECT CogitationId, RecordId, CogitationDate, Cogitation AS [Text] FROM Cogitations WHERE CogitationId = @cogitationId", new { cogitationId })).FirstOrDefault();
            }
        }

        public async Task<List<Cogitation>> FetchAllCogitationsOfRecord(int recordId)
        {
            using (var connection = await _manager.GetConnection())
            {
                return (await connection.QueryAsync<Cogitation>("SELECT CogitationId, RecordId, CogitationDate, Cogitation AS [Text] FROM Cogitations WHERE RecordId = @recordId", new { recordId })).ToList();
            }
        }

        public async Task<int> AddCogitation(Cogitation cogitation)
        {
            using (var connection = await _manager.GetConnection())
            {
                return await connection.ExecuteScalarAsync<int>("INSERT INTO Cogitations (RecordId, CogitationDate, Cogitation) VALUES (@RecordId, @CogitationDate, @Text)", cogitation);
            }
        }

        public async Task<int> UpdateCogitation(Cogitation cogitation)
        {
            using (var connection = await _manager.GetConnection())
            {
                await connection.ExecuteScalarAsync("UPDATE Cogitations SET CogitationDate = @CogitationDate, Cogitation = @Text", cogitation);
                return cogitation.CogitationId;
            }
        }

        public async Task<int> GetCogitationsCount(int recordId)
        {
            using (var connection = await _manager.GetConnection())
            {
                return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Cogitations WHERE RecordId = @recordId", new { recordId });
            }
        }
    }
}
