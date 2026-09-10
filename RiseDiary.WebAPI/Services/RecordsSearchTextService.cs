using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using RiseDiary.Common.Search;
using RiseDiary.Data;

namespace RiseDiary.Model.Services;

public sealed partial class RecordsSearchTextService : IRecordsSearchTextService
{
    private readonly DiaryDbContext _context;
    private readonly ILogger<RecordsSearchTextService> _logger;

    public RecordsSearchTextService(DiaryDbContext context, ILogger<RecordsSearchTextService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
    }

    private async Task<IEnumerable<RecordEntity>> SearchRecords(string? searchText, CancellationToken cancellationToken)
    {
        searchText = searchText ?? throw new ArgumentNullException(nameof(searchText));
        _logger.LogInformation("Поиск записей по тексту: {searchText}", searchText);

        var searchParts = SplitSearchtext().Matches(searchText)
            .Select(x => x.Value.Replace('"', ' ').Trim())
            .Where(x => x != string.Empty && x.Length > 1)
            .Distinct()
            .ToArray();

        _logger.LogInformation("Параметры поиска записей: {searchText}", (object)searchParts);


        var prelimData = await _context.Records
            .AsNoTracking()
            .Select(r => new { r.Id, r.Name, r.Text })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var recordsIds = prelimData
            .Where(pd =>
                searchParts.Any(y => pd.Name?.Contains(y, StringComparison.OrdinalIgnoreCase) ?? false) ||
                searchParts.Any(y => pd.Text?.Contains(y, StringComparison.OrdinalIgnoreCase) ?? false))
            .Select(pd => pd.Id)
            .ToList();

        var cogitationsPrelimData = await _context.Cogitations
            .AsNoTracking()
            .Select(c => new { c.RecordId, c.Text })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var recordsIds2 = cogitationsPrelimData
            .Where(pd => searchParts.Any(y => pd.Text?.Contains(y, StringComparison.OrdinalIgnoreCase) ?? false))
            .Select(pd => pd.RecordId)
            .Distinct()
            .ToList();

        var combinedList = recordsIds.Union(recordsIds2).ToList();

        if (combinedList.Count == 0) return [];

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

    public async Task<List<RecordEntity>> GetRecordsList(RecordsTextFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);
        if (string.IsNullOrWhiteSpace(filter.SearchText)) return Enumerable.Empty<RecordEntity>().ToList();

        var list = (await SearchRecords(filter.SearchText, cancellationToken).ConfigureAwait(false))
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.CreateDate)
            .Skip(filter.PageNo * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return list;
    }

    [GeneratedRegex(@"[^\s""']+|""([^""]*)""|'([^']*)'", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline)]
    private static partial Regex SplitSearchtext();
}
