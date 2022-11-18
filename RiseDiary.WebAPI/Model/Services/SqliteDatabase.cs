using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RiseDiary.Data;
using RiseDiary.Shared.Database;
using RiseDiary.WebAPI.Config;

namespace RiseDiary.Model.Services;

internal sealed class SqliteDatabase : ISqliteDatabase
{
    private readonly SqliteOptions _options;

    private readonly DiaryDbContext _context;

    public SqliteDatabase(IOptions<SqliteOptions> options, DiaryDbContext context)
    {
        _options = options.Value;
        _context = context;
    }

    private static async Task ExecuteDeleteForEntities(IQueryable<IDeletedEntity> dbSet) =>
        await dbSet.IgnoreQueryFilters()
            .AsNoTracking()
            .Where(r => r.Deleted)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);

    public async Task ClearDatabase()
    {
        await ExecuteDeleteForEntities(_context.Records);

        await ExecuteDeleteForEntities(_context.Images);

        await ExecuteDeleteForEntities(_context.Scopes);

        await ExecuteDeleteForEntities(_context.Themes);

        await ExecuteDeleteForEntities(_context.RecordImages);

        await ExecuteDeleteForEntities(_context.RecordThemes);

        await ExecuteDeleteForEntities(_context.Cogitations);
    }

    public async Task<DeletedEntitiesCount> GetDeletedEntitiesCount(CancellationToken cancellationToken)
    {
        int records = await _context.Records
            .IgnoreQueryFilters()
            .AsNoTracking()
            .CountAsync(r => r.Deleted, cancellationToken)
            .ConfigureAwait(false);

        int images = await _context.Images
            .IgnoreQueryFilters()
            .AsNoTracking()
            .CountAsync(i => i.Deleted, cancellationToken)
            .ConfigureAwait(false);

        int scopes = await _context.Scopes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .CountAsync(s => s.Deleted, cancellationToken)
            .ConfigureAwait(false);

        int themes = await _context.Themes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .CountAsync(t => t.Deleted, cancellationToken)
            .ConfigureAwait(false);

        int recImages = await _context.RecordImages
            .IgnoreQueryFilters()
            .AsNoTracking()
            .CountAsync(ri => ri.Deleted, cancellationToken)
            .ConfigureAwait(false);

        int recThemes = await _context.RecordThemes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .CountAsync(rt => rt.Deleted, cancellationToken)
            .ConfigureAwait(false);

        int cogitations = await _context.Cogitations
            .IgnoreQueryFilters()
            .AsNoTracking()
            .CountAsync(r => r.Deleted, cancellationToken)
            .ConfigureAwait(false);

        return new DeletedEntitiesCount(scopes, themes, records, cogitations, images, recThemes, recImages);
    }

    public SqliteDatabaseFileInfo GetSqliteDatabaseInfo()
    {
        var dataBaseFileName = _options.FileName;
        var dataBaseFileSize = Math.Round(new FileInfo(dataBaseFileName).Length / 1024f / 1024f, 2).ToString(CultureInfo.InvariantCulture) + " Mb";

        return new SqliteDatabaseFileInfo(dataBaseFileName, dataBaseFileSize);
    }

    public async Task Vacuum() => await _context.Database.ExecuteSqlRawAsync("vacuum;").ConfigureAwait(false);

    public async Task<DeletedDataInfo> GetDeletedEntitiesData(CancellationToken cancellationToken)
    {
        var deletedData = new DeletedDataInfo();

        deletedData.Records = await _context.Records
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Deleted)
            .Select(x => new DeletedRecord(x.Id, x.Date, x.Name, x.Text))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        deletedData.RecordsCogitations = await _context.Cogitations
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Deleted)
            .Select(x => new DeletedCogitation(x.Id, x.RecordId, x.Date, x.Text, x.Record != null ? x.Record.Name : ""))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        deletedData.RecordsThemes = await _context.RecordThemes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Deleted)
            .Select(x => new DeletedRecordTheme(x.RecordId, x.ThemeId,
                x.Record != null ? x.Record.Name : "",
                x.Theme != null ? x.Theme.ThemeName : ""))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        deletedData.RecordsImages = await _context.RecordImages
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Deleted)
            .Select(x => new DeletedRecordImage(x.RecordId, x.ImageId,
                x.Record != null ? x.Record.Name : "",
                x.Image != null ? x.Image.GetBase64Thumbnail() : ""))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        deletedData.Scopes = await _context.Scopes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Deleted)
            .Select(x => new DeletedScope(x.Id, x.ScopeName))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        deletedData.Themes = await _context.Themes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Deleted)
            .Select(x => new DeletedTheme(x.Id, x.Scope != null ? x.Scope.ScopeName : "", x.ThemeName))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        deletedData.Images = await _context.Images
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Deleted)
            .Select(x => new DeletedImage(x.Id, x.Name, x.GetBase64Thumbnail()))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return deletedData;
    }
}
