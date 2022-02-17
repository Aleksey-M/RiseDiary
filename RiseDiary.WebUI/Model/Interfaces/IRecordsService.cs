using System;
using System.Threading.Tasks;

namespace RiseDiary.Model
{
    public interface IRecordsService
    {
        Task<Guid> AddRecord(DateOnly date, string recordName, string recordText);

        Task UpdateRecord(Guid recordId, DateOnly? newDate, string? newName, string? newText);

        Task DeleteRecord(Guid recordId);

        Task<DiaryRecord> FetchRecordById(Guid recordId);

        Task<Guid> AddCogitation(Guid recordId, string cogitationText);

        Task UpdateCogitationText(Guid cogitationId, string newText);

        Task DeleteCogitation(Guid cogitationId);
    }

    public class RecordNotFoundException : Exception
    {
        public RecordNotFoundException(Guid imageId) : base($"Record with id '{imageId}' does not exist") { }
    }
}
