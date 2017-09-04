using RiseDiary.Domain.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Domain.Repositories
{
    public interface IRecordTypesRepository
    {
        Task<int> AddRecordType(int areaId, string recordTypeName);
        Task DeleteRecordType(int recordTypeId);
        Task<DiaryRecordType> FetchRecordTypeById(int recordTypeId);
        Task<List<DiaryRecordType>> FetchRecordTypesOfArea(int? areaId);
        Task<int> GetRecordTypesCount(int? areaId);
        Task<List<DiaryRecordTypeJoined>> FetchRecordTypesWithAreas();
        Task<int> UpdateRecordType(DiaryRecordType recordType);
    }
}