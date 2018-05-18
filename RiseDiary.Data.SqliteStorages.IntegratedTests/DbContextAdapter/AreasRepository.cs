using RiseDiary.Data.SqliteStorages.IntegratedTests.Repositories;
using RiseDiary.Data.SqliteStorages.IntegratedTests.TestDomain;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests.DbContextAdapter
{
    public class AreasRepository : IAreasRepository
    {
        private readonly DiaryDbContext _context;
        public AreasRepository(DiaryDbContext context)
        {
            _context = context;
        }
        public Task<int> AddArea(string areaName)
        {
            return _context.AddScope(areaName);
        }

        public Task<bool> CanDeleteArea(int areaId)
        {
            return _context.CanDeleteScope(areaId);
        }

        public Task DeleteArea(int areaId)
        {
            return _context.DeleteScope(areaId);
        }

        public async Task<List<DiaryArea>> FetchAllAreas()
        {
            var list = await _context.FetchAllScopes();
            return list.Select(sc => new DiaryArea { AreaId = sc.Id, AreaName = sc.ScopeName }).ToList();
        }

        public async Task<DiaryArea> FetchAreaById(int areaId)
        {
            var scope = await _context.FetchScopeById(areaId);
            return scope == null ? null : new DiaryArea { AreaId = scope.Id, AreaName = scope.ScopeName };
        }

        public Task<int> GetAreasCount()
        {
            return _context.GetScopesCount();
        }

        public Task<int> UpdateArea(DiaryArea area)
        {
            return _context.UpdateScope(new DiaryScope { Id = area.AreaId, ScopeName = area.AreaName });
        }        
    }
}
