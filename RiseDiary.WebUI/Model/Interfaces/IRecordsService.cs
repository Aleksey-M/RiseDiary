using System;
using System.Threading.Tasks;

namespace RiseDiary.Model
{
    public interface IRecordsService
    {
        Task<Guid> AddRecord(DateTime date, string recordName, string recordText);

        Task UpdateRecord(Guid recordId, DateTime? newDate, string? newName, string? newText);

        Task DeleteRecord(Guid recordId);

        Task<DiaryRecord> FetchRecordById(Guid recordId);

        Task<Guid> AddCogitation(Guid recordId, string cogitationText);

        Task UpdateCogitationText(Guid cogitationId, string newText);

        Task DeleteCogitation(Guid cogitationId);
    }
}
