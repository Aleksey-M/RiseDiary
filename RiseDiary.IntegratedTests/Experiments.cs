using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RiseDiary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.IntegratedTests
{
    [TestFixture]
    internal class Experiments : TestFixtureBase
    {
        [Test]
        public async Task CheckEntityFrameworkQuert()
        {
            var context = CreateContext();
            Create_20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
            var filter = new RecordsFilter();
            filter.AddThemeId(Guid.NewGuid());

            IQueryable<DiaryRecord> result;
            List<DiaryRecord>? r = null;

            if (!filter.IsEmptyTypeFilter)
            {
                var temp = context.RecordThemes
                    .Where(rt => filter.RecordThemeIds.Contains(rt.ThemeId))
                    .Select(r => new { r.RecordId, r.ThemeId })
                    .ToList()
                    .GroupBy(r => r.RecordId)
                    .Where(g => filter.RecordThemeIds.All(id => g.Select(r => r.ThemeId).Contains(id)))
                    .Select(g => g.Key);

                result = context.Records.Where(r => temp.Contains(r.Id));
            }
            else
            {
                result = context.Records.AsQueryable();
            }

            if (!RecordsFilter.IsEmpty(filter))
            {
                if (!string.IsNullOrWhiteSpace(filter.RecordNameFilter))
                {
                    result = result.Where(r => r.Name.Contains(filter.RecordNameFilter, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.RecordDateFrom != null)
                {
                    result = result.Where(r => r.Date >= filter.RecordDateFrom);
                }
                if (filter.RecordDateTo != null)
                {
                    result = result.Where(r => r.Date <= filter.RecordDateTo);
                }
            }

            var list = r ?? await result.ToListAsync();

            Assert.IsEmpty(list);
        }
    }
}
