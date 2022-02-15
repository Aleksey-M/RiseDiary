using Microsoft.EntityFrameworkCore;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.Model.Services
{
    public class RecordsSearchService : IRecordsSearchService
    {
        protected readonly DiaryDbContext _context;
        protected readonly IAppSettingsService _appSettingsService;

        public RecordsSearchService(DiaryDbContext context, IAppSettingsService appSettingsService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        }

        public async Task<List<DiaryRecord>> GetRecordsList(RecordsFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            var recordsPage = await FetchRecordsListFilteredQuery(filter)
                .OrderByDescending(r => r.Date)
                .ThenByDescending(r => r.CreateDate)
                .Skip(filter.PageNo * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync()
                .ConfigureAwait(false);

            var placeholder = _appSettingsService.GetHostAndPortPlaceholder();
            var currentHostAndPort = await _appSettingsService.GetHostAndPort();

            foreach (var rec in recordsPage)
            {
                rec.Text = rec.Text?.Replace(placeholder, currentHostAndPort, StringComparison.OrdinalIgnoreCase) ?? "";
                rec.Name = rec.Name?.Replace(placeholder, currentHostAndPort, StringComparison.OrdinalIgnoreCase) ?? "";
                foreach (var cog in rec.Cogitations)
                {
                    cog.Text = cog.Text?.Replace(placeholder, currentHostAndPort, StringComparison.OrdinalIgnoreCase) ?? "";
                }
            }

            return recordsPage;
        }

        public async Task<int> GetRecordsCount(RecordsFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            return await FetchRecordsListFilteredQuery(filter).CountAsync().ConfigureAwait(false);
        }

        protected IQueryable<DiaryRecord> FetchRecordsListFilteredQuery(RecordsFilter filter)
        {
            var result = _context.Records
                .AsNoTracking()
                .Include(r => r.Cogitations)
                .Include(r => r.ThemesRefs)
                .ThenInclude(rt => rt.Theme)
                .Include(r => r.ImagesRefs.OrderBy(x => x.Order))
                .ThenInclude(ri => ri.Image)
                .AsQueryable();


            if (!filter.IsEmptyTypeFilter)
            {
                IEnumerable<Guid> preliminaryRecordsList;

                if (filter.CombineThemes)
                {
                    preliminaryRecordsList = _context.RecordThemes
                        .AsNoTracking()
                        .Where(rt => filter.Themes.Contains(rt.ThemeId))
                        .Select(rt => rt.RecordId)
                        .Distinct();
                }
                else
                {
                    preliminaryRecordsList = _context.RecordThemes
                        .AsNoTracking()
                        .Where(rt => filter.Themes.Contains(rt.ThemeId))
                        .Select(r => new { r.RecordId, r.ThemeId })
                        .ToList()
                        .GroupBy(r => r.RecordId)
                        .Where(g => filter.Themes.All(id => g.Select(r => r.ThemeId).Contains(id)))
                        .Select(g => g.Key);
                }

                result = result.Where(r => preliminaryRecordsList.Contains(r.Id));
            }

            if (!RecordsFilter.IsEmpty(filter))
            {
                if (!string.IsNullOrWhiteSpace(filter.FilterName))
                {
                    IEnumerable<Guid> recordsWithMatchedNames = _context.Records
                        .AsNoTracking()
                        .Select(r => new { r.Id, r.Name })
                        .ToList()
                        .Where(r => r.Name.ToUpper().Contains(filter.FilterName.ToUpper()))
                        .Select(r => r.Id);

                    result = result.Where(r => recordsWithMatchedNames.Contains(r.Id));
                }

                if (filter.FromDate != null)
                {
                    result = result.Where(r => r.Date >= filter.FromDate);
                }

                if (filter.ToDate != null)
                {
                    result = result.Where(r => r.Date <= filter.ToDate);
                }
            }

            return result;
        }
    }
}
