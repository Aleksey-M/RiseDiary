using Microsoft.EntityFrameworkCore;
using RiseDiary.Data;

namespace RiseDiary.Model.Services;

internal sealed class RecordsSearchTextService : IRecordsSearchTextService
{
    private readonly DiaryDbContext _context;

    public RecordsSearchTextService(DiaryDbContext context, IAppSettingsService appSettingsService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private async Task<IEnumerable<DiaryRecord>> SearchRecords(string searchText, CancellationToken cancellationToken)
    {
        searchText = searchText?.ToUpper() ?? throw new ArgumentNullException(nameof(searchText));

        var prelimData = await _context.Records
            .AsNoTracking()
            .Select(r => new { r.Id, r.Name, r.Text })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var recordsIds = prelimData
            .Where(pd =>
                (pd.Name?.ToUpper()?.Contains(searchText) ?? false) ||
                (pd.Text?.ToUpper()?.Contains(searchText) ?? false))
            .Select(pd => pd.Id)
            .ToList();

        var cogitationsPrelimData = await _context.Cogitations
            .AsNoTracking()
            .Select(c => new { c.RecordId, c.Text })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var recordsIds2 = cogitationsPrelimData
            .Where(pd => pd.Text?.ToUpper()?.Contains(searchText) ?? false)
            .Select(pd => pd.RecordId)
            .Distinct()
            .ToList();

        var combinedList = recordsIds.Union(recordsIds2).ToList();

        if (combinedList.Count == 0) return Enumerable.Empty<DiaryRecord>();

        return await _context.Records
            .AsNoTracking()
            .Include(r => r.Cogitations)
            .Include(r => r.ThemesRefs)
            .ThenInclude(rt => rt.Theme)
            .Include(r => r.ImagesRefs.OrderBy(x => x.Order))
            .ThenInclude(ri => ri.Image)
            .Where(r => combinedList.Contains(r.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<int> GetRecordsCount(string? searchText, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchText)) return 0;
        return (await SearchRecords(searchText, cancellationToken).ConfigureAwait(false)).Count();
    }

    public async Task<List<DiaryRecord>> GetRecordsList(RecordsTextFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);
        if (string.IsNullOrWhiteSpace(filter.SearchText)) return Enumerable.Empty<DiaryRecord>().ToList();

        var list = (await SearchRecords(filter.SearchText, cancellationToken).ConfigureAwait(false))
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.CreateDate)
            .Skip(filter.PageNo * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return list;
    }
}
