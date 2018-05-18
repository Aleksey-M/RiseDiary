using RiseDiary.Data.SqliteStorages.IntegratedTests.TestDomain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests.Repositories
{
    public interface IRecordTypesRepository
    {
        Task<int> AddRecordType(int areaId, string recordTypeName);
        Task DeleteRecordType(int recordTypeId);
        Task<DiaryRecordType> FetchRecordTypeById(int recordTypeId);
        Task<List<DiaryRecordType>> FetchRecordTypesOfArea(int? areaId);
        Task<List<int>> FetchRecordTypesIds(int? areaId);
        Task<int> GetRecordTypesCount(int? areaId);
        Task<List<DiaryRecordTypeJoined>> FetchRecordTypesWithAreas();
        Task<int> UpdateRecordType(DiaryRecordType recordType);

        Task AddTypeForRecord(int recordId, int typeId);
        Task RemoveTypeForRecord(int recordId, int typeId);
        Task<List<DiaryRecordType>> FetchTypesForRecord(int recordId);
    }
}