using RiseDiary.Domain.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Domain.Repositories
{
    public interface IAreasStorage
    {
        Task<int> AddArea(string areaName);
        Task<bool> CanDeleteArea(int areaId);
        Task DeleteArea(int areaId);
        Task<DiaryArea> GetArea(int areaId);
        Task<List<DiaryArea>> GetAreas();
        Task<int> GetAreasCount();
        Task<int> UpdateArea(DiaryArea area);
    }
}