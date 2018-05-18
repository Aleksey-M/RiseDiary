using RiseDiary.Data.SqliteStorages.IntegratedTests.Repositories;
using RiseDiary.Data.SqliteStorages.IntegratedTests.TestDomain;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests.DbContextAdapter
{
    public class RecordTypesRepository : IRecordTypesRepository
    {
        private readonly DiaryDbContext _context;
        public RecordTypesRepository(DiaryDbContext context)
        {
            _context = context;
        }
        public Task<int> AddRecordType(int areaId, string recordTypeName)
        {
            return _context.AddTheme(areaId, recordTypeName);
        }

        public Task AddTypeForRecord(int recordId, int typeId)
        {
            return _context.AddRecordTheme(recordId, typeId);
        }

        public Task DeleteRecordType(int recordTypeId)
        {
            return _context.DeleteTheme(recordTypeId);
        }

        public async Task<DiaryRecordType> FetchRecordTypeById(int recordTypeId)
        {
            var theme = await _context.FetchThemeById(recordTypeId);
            if (theme == null) return null;
            return new DiaryRecordType { AreaId = theme.DiaryScopeId, RecordTypeId = theme.Id, RecordTypeName = theme.ThemeName };
        }

        public Task<List<int>> FetchRecordTypesIds(int? areaId)
        {
            return _context.FetchThemesIds(areaId);
        }

        public async Task<List<DiaryRecordType>> FetchRecordTypesOfArea(int? areaId)
        {
            var resList = await _context.FetchThemesOfScope(areaId);
            return resList.Select(rt => new DiaryRecordType {
                AreaId = rt.DiaryScopeId,
                RecordTypeId = rt.Id,
                RecordTypeName = rt.ThemeName }).ToList();
        }

        public async Task<List<DiaryRecordTypeJoined>> FetchRecordTypesWithAreas()
        {
            var resList = await _context.FetchThemesWithScopes();
            return resList.Select(rtj => new DiaryRecordTypeJoined
            {
                AreaId = rtj.ScopeId,
                AreaName = rtj.ScopeName,
                RecordTypeId = rtj.Id,
                RecordTypeName = rtj.ThemeName
            }).ToList();
        }

        public async Task<List<DiaryRecordType>> FetchTypesForRecord(int recordId)
        {
            var resList = await _context.FetchRecordThemes(recordId);
            return resList.Select(rt => new DiaryRecordType
            {
                AreaId = rt.DiaryScopeId,
                RecordTypeId = rt.Id,
                RecordTypeName = rt.ThemeName
            }).ToList();
        }

        public Task<int> GetRecordTypesCount(int? areaId)
        {
            return _context.GetThemesCount(areaId);
        }

        public Task RemoveTypeForRecord(int recordId, int typeId)
        {
            return _context.RemoveRecordTheme(recordId, typeId);
        }

        public Task<int> UpdateRecordType(DiaryRecordType recordType)
        {
            return _context.UpdateTheme(new Model.DiaryTheme { Id = recordType.RecordTypeId, DiaryScopeId = recordType.AreaId, ThemeName = recordType.RecordTypeName });
        }
    }
}
