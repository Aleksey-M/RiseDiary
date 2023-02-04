using Microsoft.EntityFrameworkCore;
using RiseDiary.Data;

namespace RiseDiary.Model.Services;

internal sealed class RecordsService : IRecordsService
{
    private readonly DiaryDbContext _context;

    public RecordsService(DiaryDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Guid> AddRecord(DateOnly date, string recordName, string recordText)
    {
        if (string.IsNullOrWhiteSpace(recordText)) throw new ArgumentException("Text should be passed for creating new record");

        var record = new DiaryRecord
        {
            Id = Guid.NewGuid(),
            CreateDate = DateTime.UtcNow,
            ModifyDate = DateTime.UtcNow,
            Date = date,
            Name = recordName,
            Text = recordText
        };

        await _context.Records.AddAsync(record).ConfigureAwait(false);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return record.Id;
    }

    public async Task DeleteRecord(Guid recordId)
    {
        var record = await _context.Records
            .Include(r => r.Cogitations)
            .Include(r => r.ImagesRefs)
            .Include(r => r.ThemesRefs)
            .FirstOrDefaultAsync(r => r.Id == recordId)
            .ConfigureAwait(false);

        if (record != null)
        {
            _context.Records.Remove(record);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public async Task<DiaryRecord> FetchRecordById(Guid recordId, CancellationToken cancellationToken)
    {
        var record = await _context.Records
           .AsNoTracking()
           .Include(r => r.Cogitations)
           .Include(r => r.ImagesRefs.OrderBy(x => x.Order))
           .ThenInclude(ir => ir.Image)
           .Include(r => r.ThemesRefs)
           .ThenInclude(rt => rt.Theme)
           .SingleOrDefaultAsync(r => r.Id == recordId, cancellationToken)
           .ConfigureAwait(false);

        if (record == null) throw new RecordNotFoundException(recordId);

        return record;
    }

    public async Task UpdateRecord(Guid recordId, DateOnly? newDate, string? newName, string? newText)
    {
        if (newDate == null && newName == null && newText == null) return;

        var record = await _context.Records.SingleOrDefaultAsync(r => r.Id == recordId).ConfigureAwait(false);
        if (record == null) throw new RecordNotFoundException(recordId);

        if (newDate != null)
        {
            record.Date = newDate.Value;
        }

        if (newName != null)
        {
            record.Name = newName;
        }

        if (newText != null)
        {
            record.Text = newText;
        }

        record.ModifyDate = DateTime.UtcNow;
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }
}
