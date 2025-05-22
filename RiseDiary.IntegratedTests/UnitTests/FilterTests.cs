using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RiseDiary.Model;

namespace RiseDiary.UnitTests;

#pragma warning disable CA1812
[TestFixture]
class FilterTests
{
    [Test]
    public void Filter_SetDateTo_ShouldCleareTime()
    {
        var recFilter = RecordsFilter.Empty;
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        recFilter.ToDate = date;

        recFilter.ToDate.Should().Be(date);
    }

    [Test]
    public void Filter_SetDateFrom_ShouldCleareTime()
    {
        var recFilter = RecordsFilter.Empty;
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        recFilter.FromDate = date;

        recFilter.FromDate.Should().Be(date);
    }

    [Test]
    public void Filter_AddRecTypesIds_ShouldAddId()
    {
        var recFilter = RecordsFilter.Empty;
        var itemId1 = Guid.NewGuid();
        var itemId2 = Guid.NewGuid();

        recFilter.AddThemeId(itemId1);
        recFilter.AddThemeId(itemId2);

        recFilter.Themes.Count.Should().Be(2);
        recFilter.Themes.Contains(itemId1).Should().BeTrue();
        recFilter.Themes.Contains(itemId2).Should().BeTrue();
    }

    [Test]
    public void Filter_AddExistingRecTypesIds_ShouldNotAddId()
    {
        var recFilter = RecordsFilter.Empty;
        var itemId1 = Guid.NewGuid();

        recFilter.AddThemeId(itemId1);
        recFilter.AddThemeId(itemId1);// same id

        recFilter.Themes.Count.Should().Be(1);
        recFilter.Themes[0].Should().Be(itemId1);
    }

    [Test]
    public void Filter_AddRecTypesList_ShouldAddId()
    {
        var recFilter = RecordsFilter.Empty;
        var recList = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        recFilter.AddThemeId(recList[4]);
        recFilter.AddThemeId(recList[0]);
        recFilter.AddThemeId(recList);

        recFilter.Themes.Count.Should().Be(recList.Length);
        recFilter.Themes.Should().BeSubsetOf(recList);
        recList.Should().BeSubsetOf(recFilter.Themes);
    }

    [Test]
    public void Filter_RemoveNotExistingId_ShouldDoNothing()
    {
        var recFilter = RecordsFilter.Empty;
        var id = Guid.NewGuid();

        recFilter.AddThemeId(id);

        recFilter.RemoveThemeId(Guid.NewGuid());

        recFilter.Themes.Count.Should().Be(1);
        recFilter.Themes[0].Should().Be(id);
    }

    [Test]
    public void Filter_RemoveId_ShouldRemoveId()
    {
        var recFilter = RecordsFilter.Empty;
        var id = Guid.NewGuid();

        recFilter.AddThemeId(id);

        recFilter.RemoveThemeId(id);

        recFilter.Themes.Should().BeEmpty();
    }

    [Test]
    public void Filter_RemoveIdList_ShouldRemoveId()
    {
        var recFilter = RecordsFilter.Empty;
        var recList = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();
        var id4 = recList[2];

        recFilter.AddThemeId(recList);
        recFilter.AddThemeId(id1);
        recFilter.AddThemeId(id2);

        recList.Remove(id4);
        recList.Add(id3);
        recFilter.RemoveThemeId(recList);

        recFilter.Themes.Count.Should().Be(3);
        recFilter.Themes.Should().Contain(id4);
        recFilter.Themes.Should().Contain(id1);
        recFilter.Themes.Should().Contain(id2);
    }

    [Test]
    public void Filter_SetDateToProperty_ShouldSet()
    {
        var filter = RecordsFilter.Empty;
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3));

        filter.ToDate = date;
        date.Should().Be(filter.ToDate);
        filter.ToDate = null;
        filter.ToDate.Should().BeNull();
    }

    [Test]
    public void Filter_SetDateFromProperty_ShouldSet()
    {
        var filter = RecordsFilter.Empty;
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));

        filter.FromDate = date;
        date.Should().Be(filter.FromDate);
        filter.FromDate = null;
        filter.FromDate.Should().BeNull();
    }

    [Test]
    public void Filter_SetDateFromAndDateToProperties_TheSameDate_ShouldSet()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));

        var filter = new RecordsFilter { FromDate = date, ToDate = date };

        date.Should().Be(filter.FromDate);
        date.Should().Be(filter.ToDate);
        filter.FromDate.Should().Be(filter.ToDate);
        filter.FromDate = null;
        filter.ToDate = null;
        filter.FromDate.Should().BeNull();
        filter.ToDate.Should().BeNull();
    }

    [Test]
    public void Filter_SetDateFrom_GreaterThanDateTo_ShouldSetToNull()
    {
        var filter = RecordsFilter.Empty;
        var dateTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var dateFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3));

        filter.FromDate = dateFrom;
        filter.ToDate = dateTo;

        dateFrom.Should().Be(filter.FromDate);
        filter.ToDate.Should().BeNull();
    }

    [Test]
    public void Filter_SetDateTo_LessThanDateFrom_ShouldSetToNull()
    {
        var filter = RecordsFilter.Empty;
        var dateTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var dateFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3));

        filter.ToDate = dateTo;
        filter.FromDate = dateFrom;

        dateTo.Should().Be(filter.ToDate);
        filter.FromDate.Should().BeNull();
    }
}
