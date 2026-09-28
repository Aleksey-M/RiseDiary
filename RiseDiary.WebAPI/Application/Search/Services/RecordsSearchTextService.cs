using Microsoft.EntityFrameworkCore;
using RiseDiary.Common.Core;
using RiseDiary.Common.Search;
using RiseDiary.Data;
using RiseDiary.WebAPI.Application.Records.Model;
using RiseDiary.WebAPI.Application.Search.Abstraction;
using System.Text.RegularExpressions;

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

    private async Task<IQueryable<RecordEntity>> SearchRecords(string? searchText, CancellationToken cancellationToken)
    {
        searchText = searchText ?? throw new ArgumentNullException(nameof(searchText));
        _logger.LogInformation("Поиск записей по тексту: {searchText}", searchText);

        var searchParts = SplitSearchText().Matches(searchText)
            .Select(x => x.Value.Replace("\"", "").Replace("'", "").Trim())
            .Where(x => x != string.Empty && x.Length > 1)
            .Distinct()
            .ToArray();

        _logger.LogInformation("Параметры поиска записей: {searchText}", (object)searchParts);


        var prelimData = await _context.Records
            .AsNoTracking()
            .Select(r => new
            {
                r.Id,
                Name = r.Name.Replace("\"", "").Replace("'", ""),
                Text = r.Text.Replace("\"", "").Replace("'", "")
            })
            .ToListAsync(cancellationToken);

        var recordsIds = prelimData
            .Where(pd =>
                searchParts.Any(y => pd.Name?.Contains(y, StringComparison.OrdinalIgnoreCase) ?? false) ||
                searchParts.Any(y => pd.Text?.Contains(y, StringComparison.OrdinalIgnoreCase) ?? false))
            .Select(pd => pd.Id)
            .ToList();

        var cogitationsPrelimData = await _context.Cogitations
            .AsNoTracking()
            .Select(c => new { c.RecordId, Text = c.Text.Replace("\"", "").Replace("'", "") })
            .ToListAsync(cancellationToken);

        var recordsIds2 = cogitationsPrelimData
            .Where(pd => searchParts.Any(y => pd.Text?.Contains(y, StringComparison.OrdinalIgnoreCase) ?? false))
            .Select(pd => pd.RecordId)
            .Distinct()
            .ToList();

        var combinedList = recordsIds.Union(recordsIds2).ToList();

        if (combinedList.Count == 0)
        {
            return Enumerable.Empty<RecordEntity>().AsQueryable();
        }

        return _context.Records
            .AsNoTracking()
            .Include(r => r.Cogitations)
            .Include(r => r.ThemesRefs)
            .ThenInclude(rt => rt.Theme)
            .Include(r => r.ImagesRefs.OrderBy(x => x.Order))
            .ThenInclude(ri => ri.Image)
            .Where(r => combinedList.Contains(r.Id));
    }

    public async Task<Result<(RecordEntity[] records, int count)>> GetRecordsList(RecordsTextFilter filter, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(filter);

            if (string.IsNullOrWhiteSpace(filter.SearchText))
            {
                return Result<(RecordEntity[] records, int count)>.Success(([], 0));
            }

            var query = await SearchRecords(filter.SearchText, cancellationToken);
            var count = query.Count();

            var page = query
                .OrderByDescending(r => r.Date)
                .ThenByDescending(r => r.CreateDate)
                .Skip(filter.PageNo * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return Result<(RecordEntity[], int)>.Success(([.. page], count));
        }
        catch (Exception exc) when (exc is not OperationCanceledException)
        {
            _logger.LogError(exc, "Возникла критическая ошибка");
            return Result<(RecordEntity[], int)>.Failure("Возникла критическая ошибка");
        }
    }

    [GeneratedRegex(@"[^\s""']+|""([^""]*)""|'([^']*)'", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline)]
    private static partial Regex SplitSearchText();
}
