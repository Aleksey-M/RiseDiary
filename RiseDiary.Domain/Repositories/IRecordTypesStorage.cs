using RiseDiary.Domain.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Domain.Repositories
{
    public interface IRecordTypesStorage
    {
        Task<int> AddRecordType(int areaId, string recordTypeName);
        Task DeleteRecordType(int recordTypeId);
        Task<DiaryRecordType> GetRecordType(int recordTypeId);
        Task<List<DiaryRecordType>> GetRecordTypes(int? areaId);
        Task<int> GetRecordTypesCount(int? areaId);
        Task<List<DiaryRecordTypeJoined>> GetRecordTypesJoined();
        Task<int> UpdateRecordType(DiaryRecordType recordType);
    }
}