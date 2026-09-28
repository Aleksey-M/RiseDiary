using Microsoft.EntityFrameworkCore;
using RiseDiary.Common.Core;
using RiseDiary.Common.Search;
using RiseDiary.Data;
using RiseDiary.WebAPI.Application.Records.Model;
using RiseDiary.WebAPI.Application.Search.Abstraction;

namespace RiseDiary.WebAPI.Application.Search.Services;

public sealed class RecordsSearchService : IRecordsSearchService
{
    private readonly DiaryDbContext _context;
    private readonly ILogger<RecordsSearchService> _logger;

    public RecordsSearchService(DiaryDbContext context, ILogger<RecordsSearchService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<(RecordEntity[] records, int count)>> GetRecords(RecordsFilter filter, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(filter);

            var query = FetchRecordsListFilteredQuery(filter);
            var count = await query
                .CountAsync(cancellationToken);

            var recordsPage = await query
                .OrderByDescending(r => r.Date)
                .ThenByDescending(r => r.CreateDate)
                .Skip(filter.PageNo * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            return Result<(RecordEntity[], int)>.Success(([.. recordsPage], count));
        }
        catch (Exception exc) when (exc is not OperationCanceledException)
        {
            _logger.LogError(exc, "Возникла критическая ошибка");
            return Result<(RecordEntity[], int)>.Failure("Возникла критическая ошибка");
        }
    }

    private IQueryable<RecordEntity> FetchRecordsListFilteredQuery(RecordsFilter filter)
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

    public async Task<Result<RecordEntity[]>> GetThisDayRecords(
        int month,
        int day,
        CancellationToken cancellationToken)
    {
        try
        {
            var records = await _context.Records
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(r => r.Cogitations)
                    .Include(r => r.ThemesRefs)
                    .ThenInclude(rt => rt.Theme)
                    .Include(r => r.ImagesRefs.OrderBy(x => x.Order))
                    .ThenInclude(ri => ri.Image)
                    .Where(x => x.Date.Month == month && x.Date.Day == day)
                    .OrderByDescending(r => r.Date)
                    .ThenByDescending(r => r.CreateDate)
                    .ToListAsync(cancellationToken);

            return Result<RecordEntity[]>.Success([.. records]);
        }
        catch (Exception exc) when (exc is not OperationCanceledException)
        {
            _logger.LogError(exc, "Возникла критическая ошибка");
            return Result<RecordEntity[]>.Failure("Возникла критическая ошибка");
        }
    }


    public async Task<Result<RecordEntity[]>> GetRecordsByIds(
        IEnumerable<Guid> recordsIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var records = await _context.Records
                .AsNoTracking()
                .AsSplitQuery()
                .Include(r => r.Cogitations)
                .Include(r => r.ThemesRefs)
                .ThenInclude(rt => rt.Theme)
                .Include(r => r.ImagesRefs.OrderBy(x => x.Order))
                .ThenInclude(ri => ri.Image)
                .Where(x => recordsIds.Contains(x.Id))
                .OrderByDescending(r => r.Date)
                .ThenByDescending(r => r.CreateDate)
                .ToListAsync(cancellationToken);

            return Result<RecordEntity[]>.Success([.. records]);
        }
        catch (Exception exc) when (exc is not OperationCanceledException)
        {
            _logger.LogError(exc, "Возникла критическая ошибка");
            return Result<RecordEntity[]>.Failure("Возникла критическая ошибка");
        }
    }
}
