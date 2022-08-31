using Microsoft.EntityFrameworkCore;
using RiseDiary.Shared;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RiseDiary.Model.Services
{
    internal sealed class DatesService : IDatesService
    {
        private readonly DiaryDbContext _context;

        private readonly IAppSettingsService _appSettingsService;

        public DatesService(DiaryDbContext context, IAppSettingsService appSettingsService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        }

        public async Task<List<DateListItem>> GetAllDates(DateOnly today, CancellationToken cancellationToken)
        {
            var sId = (await _appSettingsService.GetAppSetting(AppSettingsKey.ImportantDaysScopeId)).value ?? throw new Exception("Setting 'ImportantDaysScopeId' does not exists");
            var scopeId = Guid.Parse(sId);
            var placeholder = _appSettingsService.GetHostAndPortPlaceholder();
            var currentHostAndPort = await _appSettingsService.GetHostAndPort();

            var allRecordsIds = _context.Scopes
                .AsNoTracking()
                .Where(s => s.Id == scopeId)
                .SelectMany(s => s.Themes.SelectMany(t => t.RecordsRefs).Select(rr => rr.RecordId))
                .AsEnumerable();

            var records = await _context.Records
                .AsNoTracking()
                .Include(r => r.ThemesRefs)
                .ThenInclude(tr => tr.Theme)
                .Where(r => allRecordsIds.Contains(r.Id))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var items = records
                .Select(r => new DateListItem(
                    r.Id,
                    r.Date,
                    new DateOnly(today.Year, r.Date.Month, r.Date.Day),
                    string.IsNullOrWhiteSpace(r.Name) 
                        ? "[ПУСТО]"
                        : r.Name.Replace(placeholder, currentHostAndPort, StringComparison.OrdinalIgnoreCase),
                    r.Text?.Replace(placeholder, currentHostAndPort, StringComparison.OrdinalIgnoreCase) ?? "",
                    string.Join(", ", r.ThemesRefs.Select(tr => tr.Theme!.ThemeName))))
                .OrderBy(r => r.TransferredDate)
                .ToList();

            return items;
        }

        public async Task<List<DateListItem>> GetDatesFromRange(DateOnly today,
            bool withEmptyDates, CancellationToken cancellationToken)
        {
            var daysCount = (await _appSettingsService.GetAppSettingInt(AppSettingsKey.ImportantDaysDisplayRange))
                ?? throw new Exception("Setting 'ImportantDaysDisplayRange' does not exists");
            var allRecords = await GetAllDates(today, cancellationToken);

            var startDate = today.AddDays(-daysCount);
            var datesRange = Enumerable.Range(0, daysCount * 2)
                .Select(i => startDate.AddDays(i))
                .Select(d => new DateListItem(Guid.Empty, d, d, "", "", ""))
                .ToList();

            var datesFromRange = new List<DateListItem>();
            var emptyDates = new HashSet<DateListItem>();

            foreach(var rec in allRecords)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var emptyDate = datesRange.SingleOrDefault(d => 
                    d.TransferredDate.Month == rec.TransferredDate.Month 
                    && d.TransferredDate.Day == rec.TransferredDate.Day);
                if (emptyDate == null) continue;

                datesFromRange.Add(rec with { TransferredDate = emptyDate.TransferredDate });
                if (!emptyDates.Contains(emptyDate)) emptyDates.Add(emptyDate);
            }

            if (!withEmptyDates) return datesFromRange.OrderBy(d => d.TransferredDate).ToList();

            datesRange = datesRange.Except(emptyDates).ToList();
            datesRange.AddRange(datesFromRange);

            return datesRange.OrderBy(d => d.TransferredDate).ToList();
        }
    }
}
