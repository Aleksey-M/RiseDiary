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
        [Test, Ignore("Experimental test")]
        public async Task CheckEntityFrameworkQuert()
        {
            var context = CreateContext();
            Create20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
            var filter = new RecordsFilter();
            filter.AddThemeId(Guid.NewGuid());

            IQueryable<DiaryRecord> result;
            List<DiaryRecord>? r = null;

            if (!filter.IsEmptyTypeFilter)
            {
                var temp = context.RecordThemes
                    .Where(rt => filter.Themes.Contains(rt.ThemeId))
                    .Select(r => new { r.RecordId, r.ThemeId })
                    .ToList()
                    .GroupBy(r => r.RecordId)
                    .Where(g => filter.Themes.All(id => g.Select(r => r.ThemeId).Contains(id)))
                    .Select(g => g.Key);

                result = context.Records.Where(r => temp.Contains(r.Id));
            }
            else
            {
                result = context.Records.AsQueryable();
            }

            if (!RecordsFilter.IsEmpty(filter))
            {
                if (!string.IsNullOrWhiteSpace(filter.FilterName))
                {
                    result = result.Where(r => r.Name.Contains(filter.FilterName, StringComparison.OrdinalIgnoreCase));
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

            var list = r ?? await result.ToListAsync();

            Assert.IsEmpty(list);
        }
    }
}
