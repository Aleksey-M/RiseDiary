using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

#pragma warning disable CA1812

namespace RiseDiary.IntegratedTests.Services;

[TestFixture]
internal class CalendarServiceTests : TestFixtureBase
{
    private readonly List<DateOnly> _recordDates = new()
    {
            new DateOnly(2017, 7, 5),
            new DateOnly(2018, 1, 24),
            new DateOnly(2019, 10, 19),
            new DateOnly(2017, 3, 16),
            new DateOnly(2018, 1, 4),
            new DateOnly(1999, 2, 28) };

    private readonly Dictionary<DateOnly, List<string>> _recordsDatesThemes = new()
    {
        { new DateOnly(2017, 7, 5), new List<string>{ "Important Dates", "Dreams" } },
        { new DateOnly(2018, 1, 24), new List<string>{ "Intents", "Important Dates" } },
        { new DateOnly(2019, 10, 19), new List<string>{ "Dreams" } },
        { new DateOnly(2017, 3, 16), new List<string>{ "Important Dates" } },
        { new DateOnly(2018, 1, 4), new List<string>{ "Dreams", "Other Theme", "Intents", "Important Dates" } },
        { new DateOnly(1999, 2, 28), new List<string>{ "Dreams" } }
    };

    private readonly List<Guid> _empty = Enumerable.Empty<Guid>().ToList();

    // проверка замены ссылки в названии

    [Test]
    public async Task GetYears_OnEmptyBase_ShouldReturnEmptyList()
    {
        var calendarService = GetCalendarService();

        var res = await calendarService.GetYears(_empty, false);

        res.Should().NotBeNull();
        res.Should().BeEmpty();

        //=========================//

        res = await calendarService.GetYears(_empty, true);

        res.Should().NotBeNull();
        res.Should().BeEmpty();
    }

    [Test]
    public async Task SoftDeleting_GetYears_ShouldReturn3Years()
    {
        var context = CreateContext();
        context.SoftDeleting = true;
        await AddSetOfRecordsWithDates(context, _recordDates);
        foreach (var r in context.Records.AsEnumerable().Where(r => r.Date.Year == 2018))
        {
            r.Deleted = true;
        }
        await context.SaveChangesAsync();
        var calendarService = GetCalendarService(context);

        var res = await calendarService.GetYears(_empty, false);

        res.Should().NotBeNull();
        res.Should().HaveCount(3);
        res.Should().BeInAscendingOrder();
        res.Should().BeEquivalentTo(new int[] { 1999, 2017, 2019 });

        //=========================//

        res = await calendarService.GetYears(_empty, true);

        res.Should().NotBeNull();
        res.Should().HaveCount(3);
        res.Should().BeInAscendingOrder();
        res.Should().BeEquivalentTo(new int[] { 1999, 2017, 2019 });
    }

    [Test]
    public async Task GetYears_ShouldReturn4Years()
    {
        var context = CreateContext();
        await AddSetOfRecordsWithDates(context, _recordDates);
        var calendarService = GetCalendarService(context);

        var res = await calendarService.GetYears(_empty, false);

        res.Should().NotBeNull();
        res.Should().HaveCount(4);
        res.Should().BeInAscendingOrder();
        res.Should().BeEquivalentTo(new int[] { 1999, 2017, 2018, 2019 });

        //=========================//

        res = await calendarService.GetYears(_empty, true);

        res.Should().NotBeNull();
        res.Should().HaveCount(4);
        res.Should().BeInAscendingOrder();
        res.Should().BeEquivalentTo(new int[] { 1999, 2017, 2018, 2019 });
    }

    [Test]
    public async Task GetCalendarItems_OnEmptyBase_ShouldReturnEmptyList()
    {
        var calendarService = GetCalendarService();

        var res = await calendarService.GetCalendarItems(2020, _empty, false);

        res.Should().NotBeNull();
        res.Should().BeEmpty();

        //==========================//

        res = await calendarService.GetCalendarItems(2020, _empty, true);

        res.Should().NotBeNull();
        res.Should().BeEmpty();
    }

    [Test]
    public async Task GetCalendarItems_WithNoDatesToYear_ShouldReturnEmptyList()
    {
        var context = CreateContext();
        await AddSetOfRecordsWithDates(context, _recordDates);
        var calendarService = GetCalendarService(context);

        var res = await calendarService.GetCalendarItems(2020, _empty, false);

        res.Should().NotBeNull();
        res.Should().BeEmpty();

        //======================//

        res = await calendarService.GetCalendarItems(2020, _empty, true);

        res.Should().NotBeNull();
        res.Should().BeEmpty();
    }

    [Test]
    public async Task GetCalendarItems_OnlyByYear_ShouldReturnRecordsForYear()
    {
        var context = CreateContext();
        await AddSetOfRecordsWithDates(context, _recordDates);
        var calendarService = GetCalendarService(context);

        var res = await calendarService.GetCalendarItems(2017, _empty, false);

        res.Should().NotBeNull();
        res.Should().HaveCount(2);

        //=====================//

        res = await calendarService.GetCalendarItems(2017, _empty, true);

        res.Should().NotBeNull();
        res.Should().HaveCount(2);
    }

    [Test]
    public async Task GetCalendarItems_WithNoMatchesByThemes_ShouldReturnEmptyList()
    {
        var context = CreateContext();
        var data = await AddThemesForRecords(context, _recordsDatesThemes);
        var calendarService = GetCalendarService(context);
        var themesFilter = new List<Guid> { data.ElementAt(3).themesIds.First() }; // "Important Dates"

        var res = await calendarService.GetCalendarItems(2019, _empty, false);

        res.Should().NotBeNull();
        res.Should().HaveCount(1);

        res = await calendarService.GetCalendarItems(2019, themesFilter, false);

        res.Should().NotBeNull();
        res.Should().BeEmpty();

        res = await calendarService.GetCalendarItems(2019, _empty, true);

        res.Should().NotBeNull();
        res.Should().HaveCount(1);

        res = await calendarService.GetCalendarItems(2019, themesFilter, true);

        res.Should().NotBeNull();
        res.Should().BeEmpty();
    }

    [Test]
    public async Task GetCalendarItems_WithThemesAndYearMatches_ShouldReturnRecordsForYear()
    {
        var context = CreateContext();
        var data = await AddThemesForRecords(context, _recordsDatesThemes);
        var calendarService = GetCalendarService(context);
        var themesFilter = new List<Guid> { data.ElementAt(3).themesIds.First() }; // "Important Dates"

        var res = await calendarService.GetCalendarItems(2017, themesFilter, false);

        res.Should().NotBeNull();
        res.Should().HaveCount(2);
    }


    [Test]
    public async Task GetCalendarItems_WithThemesAndYearMatches_And_CombineThemesTrue_ShouldReturnRecordsForYear()
    {
        var context = CreateContext();
        var calendarService = GetCalendarService(context);
        var _newRecordDateThemes = new Dictionary<DateOnly, List<string>>();
        foreach (var i in _recordsDatesThemes)
        {
            _newRecordDateThemes[new DateOnly(2017, i.Key.Month, i.Key.Day)] = i.Value;
        }
        var data = await AddThemesForRecords(context, _newRecordDateThemes);
        var themesFilter = new List<Guid> { data.ElementAt(3).themesIds.First(), data.ElementAt(1).themesIds.First() }; // "Important Dates" + "Intents"

        var res = await calendarService.GetCalendarItems(2017, themesFilter, true);

        res.Should().HaveCount(4);
    }

    [Test]
    public async Task GetCalendarItems_WithThemesAndYearMatches_ShouldNotReturnRecordsForDeleted()
    {
        var context = CreateContext();
        context.SoftDeleting = true;
        var calendarService = GetCalendarService(context);
        var data = await AddThemesForRecords(context, _recordsDatesThemes);
        var themesFilter = new List<Guid> { data.ElementAt(3).themesIds.First(), data.ElementAt(1).themesIds.First() }; // "Important Dates"

        var deletedDate = new DateOnly(2017, 3, 16);
        context.Records.Remove(context.Records.Single(r => r.Date == deletedDate));
        context.SaveChanges();

        var res = await calendarService.GetCalendarItems(2017, themesFilter, true);

        res.Should().HaveCount(1);

        var deletedRec = await context.Records.IgnoreQueryFilters().SingleOrDefaultAsync(r => r.Date == deletedDate);

        deletedRec.Should().NotBeNull();
        deletedRec?.Deleted.Should().BeTrue();
    }

    [Test]
    public async Task GetCalendarItems_WithThemesAndYearMatches_And_CombineThemesTrue_ShouldNotReturnRecordsForDeleted()
    {
        var context = CreateContext();
        context.SoftDeleting = true;
        var calendarService = GetCalendarService(context);
        var _newRecordDateThemes = new Dictionary<DateOnly, List<string>>();
        foreach (var i in _recordsDatesThemes)
        {
            _newRecordDateThemes[new DateOnly(2017, i.Key.Month, i.Key.Day)] = i.Value;
        }
        var data = await AddThemesForRecords(context, _newRecordDateThemes);
        var themesFilter = new List<Guid> { data.ElementAt(3).themesIds.First(), data.ElementAt(1).themesIds.First() }; // "Important Dates" + "Intents"
        var deletedDate = new DateOnly(2017, 3, 16);
        context.Records.Remove(context.Records.Single(r => r.Date == deletedDate));
        context.SaveChanges();

        var res = await calendarService.GetCalendarItems(2017, themesFilter, true);

        res.Should().HaveCount(3);

        var deletedRec = await context.Records.IgnoreQueryFilters().SingleOrDefaultAsync(r => r.Date == deletedDate);

        deletedRec.Should().NotBeNull();
        deletedRec?.Deleted.Should().BeTrue();
    }

    [Test]
    public async Task GetCalendarItems_WithYearMatches_ShouldNotReturnRecordsForDeleted()
    {
        var context = CreateContext();
        context.SoftDeleting = true;
        var calendarService = GetCalendarService(context);
        var data = AddThemesForRecords(context, _recordsDatesThemes);

        var deletedDate = new DateOnly(2017, 3, 16);
        var rec = context.Records.Single(r => r.Date == deletedDate);
        context.Records.Remove(rec);
        context.SaveChanges();

        var res = await calendarService.GetCalendarItems(2017, _empty, false);

        res.Should().NotBeNull();
        res.Should().HaveCount(1);

        var deletedRec = await context.Records.IgnoreQueryFilters().SingleOrDefaultAsync(r => r.Date == deletedDate);

        deletedRec.Should().NotBeNull();
        deletedRec?.Deleted.Should().BeTrue();
    }
}
