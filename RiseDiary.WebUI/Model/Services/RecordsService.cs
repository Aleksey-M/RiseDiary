using Microsoft.EntityFrameworkCore;
using RiseDiary.WebUI.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.Model.Services
{
    public class RecordsService : IRecordsService
    {
        private readonly DiaryDbContext _context;
        private readonly IAppSettingsService _appSettingsService;

        public RecordsService(DiaryDbContext context, IAppSettingsService appSettingsService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        }

        public async Task<Guid> AddCogitation(Guid recordId, string cogitationText)
        {
            if (string.IsNullOrWhiteSpace(cogitationText)) throw new ArgumentException("Text should be passed for creating new cogitation");

            var placeholder = _appSettingsService.GetHostAndPortPlaceholder();
            var currentHostAndPort = await _appSettingsService.GetHostAndPort();

            bool isRecordExists = await _context.Records.AnyAsync(r => r.Id == recordId).ConfigureAwait(false);
            if (!isRecordExists) throw new RecordNotFoundException(recordId);

            var cogitation = new Cogitation
            {
                Id = Guid.NewGuid(),
                RecordId = recordId,
                Date = DateTime.UtcNow,
                Text = cogitationText.Replace(currentHostAndPort, placeholder, StringComparison.OrdinalIgnoreCase)
            };

            await _context.Cogitations.AddAsync(cogitation).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return cogitation.Id;
        }

        public async Task<Guid> AddRecord(DateOnly date, string recordName, string recordText)
        {
            if (string.IsNullOrWhiteSpace(recordText)) throw new ArgumentException("Text should be passed for creating new record");

            var placeholder = _appSettingsService.GetHostAndPortPlaceholder();
            var currentHostAndPort = await _appSettingsService.GetHostAndPort();

            var record = new DiaryRecord
            {
                Id = Guid.NewGuid(),
                CreateDate = DateTime.UtcNow,
                ModifyDate = DateTime.UtcNow,
                Date = date,
                Name = (recordName ?? "").Replace(currentHostAndPort, placeholder, StringComparison.OrdinalIgnoreCase),
                Text = recordText.Replace(currentHostAndPort, placeholder, StringComparison.OrdinalIgnoreCase)
            };

            await _context.Records.AddAsync(record).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return record.Id;
        }

        public async Task DeleteCogitation(Guid cogitationId)
        {
            var cogitation = await _context.Cogitations.SingleOrDefaultAsync(c => c.Id == cogitationId).ConfigureAwait(false);
            if (cogitation != null)
            {
                _context.Cogitations.Remove(cogitation);
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
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

        public async Task<DiaryRecord> FetchRecordById(Guid recordId)
        {
            var record = await _context.Records
               .AsNoTracking()
               .Include(r => r.Cogitations)
               .Include(r => r.ImagesRefs.OrderBy(x => x.Order))
               .ThenInclude(ir => ir.Image)
               .Include(r => r.ThemesRefs)
               .ThenInclude(rt => rt.Theme)
               .SingleOrDefaultAsync(r => r.Id == recordId)
               .ConfigureAwait(false);

            if (record == null) throw new RecordNotFoundException(recordId);

            var placeholder = _appSettingsService.GetHostAndPortPlaceholder();
            var currentHostAndPort = await _appSettingsService.GetHostAndPort();

            record.Text = (record.Text ?? "").Replace(placeholder, currentHostAndPort, StringComparison.OrdinalIgnoreCase);
            record.Name = (record.Name ?? "").Replace(placeholder, currentHostAndPort, StringComparison.OrdinalIgnoreCase);

            record.Cogitations.ToList()
                .ForEach(cog => cog.Text = (cog.Text ?? "").Replace(placeholder, currentHostAndPort, StringComparison.OrdinalIgnoreCase));

            return record;
        }

        public async Task UpdateCogitationText(Guid cogitationId, string newText)
        {
            var cogitation = await _context.Cogitations.SingleOrDefaultAsync(c => c.Id == cogitationId).ConfigureAwait(false);
            if (cogitation == null) throw new ArgumentException($"Cogitation with Id='{cogitationId}' does not exists");
            if (string.IsNullOrWhiteSpace(newText)) throw new ArgumentException("Cogitation should not be empty");

            var placeholder = _appSettingsService.GetHostAndPortPlaceholder();
            var currentHostAndPort = await _appSettingsService.GetHostAndPort();

            cogitation.Text = newText.Replace(currentHostAndPort, placeholder, StringComparison.OrdinalIgnoreCase);
            await _context.SaveChangesAsync().ConfigureAwait(false);
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

            var placeholder = _appSettingsService.GetHostAndPortPlaceholder();
            var currentHostAndPort = await _appSettingsService.GetHostAndPort();

            if (newName != null)
            {
                record.Name = currentHostAndPort.Length > 0
                    ? newName.Replace(currentHostAndPort, placeholder, StringComparison.OrdinalIgnoreCase)
                    : newName;
            }

            if (newText != null)
            {
                record.Text = currentHostAndPort.Length > 0
                    ? newText.Replace(currentHostAndPort, placeholder, StringComparison.OrdinalIgnoreCase)
                    : newText;
            }

            record.ModifyDate = DateTime.UtcNow;
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
