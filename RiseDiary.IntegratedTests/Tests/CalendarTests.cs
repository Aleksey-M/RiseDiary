﻿using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.IntegratedTests
{
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning disable CA1812
    [TestFixture]
    internal class CalendarTests : TestFixtureBase
    {

        private readonly List<DateTime> _recordDates = new List<DateTime> {
                new DateTime(2017, 7, 5),
                new DateTime(2018, 1, 24),
                new DateTime(2019, 10, 19),
                new DateTime(2017, 3, 16),
                new DateTime(2018, 1, 4),
                new DateTime(1999, 2, 28) };

        private readonly Dictionary<DateTime, List<string>> _recordsDatesThemes = new Dictionary<DateTime, List<string>> {
            { new DateTime(2017, 7, 5), new List<string>{ "Important Dates", "Dreams" } },
            { new DateTime(2018, 1, 24), new List<string>{ "Intents", "Important Dates" } },
            { new DateTime(2019, 10, 19), new List<string>{ "Dreams" } },
            { new DateTime(2017, 3, 16), new List<string>{ "Important Dates" } },
            { new DateTime(2018, 1, 4), new List<string>{ "Dreams", "Other Theme", "Intents", "Important Dates" } },
            { new DateTime(1999, 2, 28), new List<string>{ "Dreams" } }
        };

        [Test]
        public async Task FetchCalendarDates_OnEmptyBase_ShouldReturnEmptyList()
        {
            var context = CreateContext();

            var res = await context.FetchCalendarDates(DateTime.Now.Year, Array.Empty<Guid>(), false);

            Assert.IsNotNull(res);
            Assert.IsEmpty(res);
        }

        [Test]
        public async Task FetchCalendarDates_WithNoDatesToYear_ShouldReturnEmptyList()
        {
            var context = CreateContext();
            await AddSetOfRecordsWithDates(context, _recordDates);

            var res = await context.FetchCalendarDates(2020, Array.Empty<Guid>(), false);

            Assert.IsNotNull(res);
            Assert.IsEmpty(res);
        }

        [Test]
        public async Task FetchCalendarDates_OnlyByYear_ShouldReturnRecordsForYear()
        {
            var context = CreateContext();
            await AddSetOfRecordsWithDates(context, _recordDates);

            var res = await context.FetchCalendarDates(2017, Array.Empty<Guid>(), false);

            Assert.IsNotNull(res);
            Assert.AreEqual(2, res.Count);
        }

        [Test]
        public async Task FetchCalendarDates_WithNoMatchesByThemes_ShouldReturnEmptyList()
        {
            var context = CreateContext();
            var data = await AddThemesForRecords(context, _recordsDatesThemes);
            var themesFilter = new[] { data.ElementAt(3).themesIds.First() }; // "Important Dates"

            var res = await context.FetchCalendarDates(2019, Array.Empty<Guid>(), false);

            Assert.IsNotNull(res);
            Assert.AreEqual(1, res.Count);

            res = await context.FetchCalendarDates(2019, themesFilter, false);

            Assert.IsNotNull(res);
            Assert.IsEmpty(res);
        }

        [Test]
        public async Task FetchCalendarDates_WithThemesAndYearMatches_ShouldReturnRecordsForYear()
        {
            var context = CreateContext();
            var data = await AddThemesForRecords(context, _recordsDatesThemes);
            var themesFilter = new[] { data.ElementAt(3).themesIds.First() }; // "Important Dates"

            var res = await context.FetchCalendarDates(2017, themesFilter, false);

            Assert.IsNotNull(res);
            Assert.AreEqual(2, res.Count);
        }

        [Test]
        public async Task FetchCalendarDates_WithThemesAndYearMatches_And_CombineThemesTrue_ShouldReturnRecordsForYear()
        {
            var context = CreateContext();
            var _newRecordDateThemes = new Dictionary<DateTime, List<string>>();
            foreach (var i in _recordsDatesThemes)
            {
                _newRecordDateThemes[new DateTime(2017, i.Key.Month, i.Key.Day)] = i.Value;
            }            
            var data = await AddThemesForRecords(context, _newRecordDateThemes);
            var themesFilter = new[] { data.ElementAt(3).themesIds.First(), data.ElementAt(1).themesIds.First() }; // "Important Dates" + "Intents"

            var res = await context.FetchCalendarDates(2017, themesFilter, true);

            Assert.IsNotNull(res);
            Assert.AreEqual(4, res.Count);
        }

        [Test]
        public async Task FetchCalendarDates_WithThemesAndYearMatches_ShouldNotReturnRecordsForDeleted()
        {
            var context = CreateContext();
            context.SoftDeleting = true;
            var data = await AddThemesForRecords(context, _recordsDatesThemes);
            var themesFilter = new[] { data.ElementAt(3).themesIds.First(), data.ElementAt(1).themesIds.First() }; // "Important Dates"

            var deletedDate = new DateTime(2017, 3, 16);
            context.Records.Remove(context.Records.Single(r => r.Date == deletedDate));
            context.SaveChanges();

            var res = await context.FetchCalendarDates(2017, themesFilter, true);

            Assert.IsNotNull(res);
            Assert.AreEqual(1, res.Count);

            var deletedRec = await context.Records.IgnoreQueryFilters().SingleOrDefaultAsync(r => r.Date == deletedDate);

            Assert.IsNotNull(deletedRec);
            Assert.IsTrue(deletedRec.Deleted);
        }

        [Test]
        public async Task FetchCalendarDates_WithThemesAndYearMatches_And_CombineThemesTrue_ShouldNotReturnRecordsForDeleted()
        {
            var context = CreateContext();
            context.SoftDeleting = true;
            var _newRecordDateThemes = new Dictionary<DateTime, List<string>>();
            foreach (var i in _recordsDatesThemes)
            {
                _newRecordDateThemes[new DateTime(2017, i.Key.Month, i.Key.Day)] = i.Value;
            }
            var data = await AddThemesForRecords(context, _newRecordDateThemes);
            var themesFilter = new[] { data.ElementAt(3).themesIds.First(), data.ElementAt(1).themesIds.First() }; // "Important Dates" + "Intents"

            var deletedDate = new DateTime(2017, 3, 16);
            context.Records.Remove(context.Records.Single(r => r.Date == deletedDate));
            context.SaveChanges();

            var res = await context.FetchCalendarDates(2017, themesFilter, true);

            Assert.IsNotNull(res);
            Assert.AreEqual(3, res.Count);

            var deletedRec = await context.Records.IgnoreQueryFilters().SingleOrDefaultAsync(r => r.Date == deletedDate);

            Assert.IsNotNull(deletedRec);
            Assert.IsTrue(deletedRec.Deleted);
        }

        [Test]
        public async Task FetchCalendarDates_WithYearMatches_ShouldNotReturnRecordsForDeleted()
        {
            var context = CreateContext();
            context.SoftDeleting = true;
            var data = AddThemesForRecords(context, _recordsDatesThemes);

            var deletedDate = new DateTime(2017, 3, 16);
            var rec = context.Records.Single(r => r.Date == deletedDate);
            context.Records.Remove(rec);
            context.SaveChanges();

            var res = await context.FetchCalendarDates(2017, Array.Empty<Guid>(), false);

            Assert.IsNotNull(res);
            Assert.AreEqual(1, res.Count);

            var deletedRec = await context.Records.IgnoreQueryFilters().SingleOrDefaultAsync(r => r.Date == deletedDate);

            Assert.IsNotNull(deletedRec);
            Assert.IsTrue(deletedRec.Deleted);
        }
    }
}
