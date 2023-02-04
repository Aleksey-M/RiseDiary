using Microsoft.EntityFrameworkCore;
using RiseDiary.Data;

namespace RiseDiary.Model.Services;

internal sealed class RecordsSearchService : IRecordsSearchService
{
    private readonly DiaryDbContext _context;

    public RecordsSearchService(DiaryDbContext context, IAppSettingsService appSettingsService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<DiaryRecord>> GetRecordsList(RecordsFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var recordsPage = await FetchRecordsListFilteredQuery(filter)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.CreateDate)
            .Skip(filter.PageNo * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return recordsPage;
    }

    public async Task<int> GetRecordsCount(RecordsFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return await FetchRecordsListFilteredQuery(filter).CountAsync(cancellationToken).ConfigureAwait(false);
    }

    private IQueryable<DiaryRecord> FetchRecordsListFilteredQuery(RecordsFilter filter)
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

    public async Task<List<DiaryRecord>> GetThisDayRecords(int month, int day, CancellationToken cancellationToken)
    {
        var records = await _context.Records
            .AsNoTracking()
            .Include(r => r.Cogitations)
            .Include(r => r.ThemesRefs)
            .ThenInclude(rt => rt.Theme)
            .Include(r => r.ImagesRefs.OrderBy(x => x.Order))
            .ThenInclude(ri => ri.Image)
            .Where(x => x.Date.Month == month && x.Date.Day == day)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.CreateDate)
            .ToListAsync(cancellationToken);

        return records;
    }
}
