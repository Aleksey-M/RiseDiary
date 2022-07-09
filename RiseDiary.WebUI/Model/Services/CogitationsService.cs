using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RiseDiary.WebUI.Data;

namespace RiseDiary.Model.Services
{
    public class CogitationsService : ICogitationsService
    {
        private readonly DiaryDbContext _context;

        private readonly IAppSettingsService _appSettingsService;

        public CogitationsService(DiaryDbContext context, IAppSettingsService appSettingsService)
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

        public async Task DeleteCogitation(Guid cogitationId)
        {
            var cogitation = await _context.Cogitations.SingleOrDefaultAsync(c => c.Id == cogitationId).ConfigureAwait(false);
            if (cogitation != null)
            {
                _context.Cogitations.Remove(cogitation);
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<List<Cogitation>> GetRecordCogitations(Guid recordId)
        {
            var placeholder = _appSettingsService.GetHostAndPortPlaceholder();
            var currentHostAndPort = await _appSettingsService.GetHostAndPort();

            var cogitations = await _context.Cogitations
                .AsNoTracking()
                .Where(x => x.RecordId == recordId)
                .OrderByDescending(x => x.Date)
                .ToListAsync()
                .ConfigureAwait(false);

            cogitations.ForEach(
                cog => cog.Text = (cog.Text ?? "").Replace(placeholder, currentHostAndPort, StringComparison.OrdinalIgnoreCase));

            return cogitations;
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
    }
}
