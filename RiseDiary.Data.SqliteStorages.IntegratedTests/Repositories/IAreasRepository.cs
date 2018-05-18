using RiseDiary.Data.SqliteStorages.IntegratedTests.TestDomain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests.Repositories
{
    public interface IAreasRepository
    {
        Task<int> AddArea(string areaName);
        Task<bool> CanDeleteArea(int areaId);
        Task DeleteArea(int areaId);
        Task<DiaryArea> FetchAreaById(int areaId);
        Task<List<DiaryArea>> FetchAllAreas();
        Task<int> GetAreasCount();
        Task<int> UpdateArea(DiaryArea area);
    }
}