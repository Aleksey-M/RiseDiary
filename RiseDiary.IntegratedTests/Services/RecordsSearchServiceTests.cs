﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RiseDiary.Model;

namespace RiseDiary.IntegratedTests.Services;

[TestFixture]
internal class RecordsSearchServiceTests : TestFixtureBase
{

    [Test]
    public async Task SoftDeleting_GetRecordsList_ShouldNotReturnDeletedRecords()
    {
        var context = CreateContext();
        context.SoftDeleting = true;
        Create20Records(context, GetNumberList(20), GetDatesList(20));
        var svc = GetRecordsSearchService(context);
        // mark one record as deleted
        var delRecord = await context.Records.Skip(3).FirstAsync();
        delRecord.Deleted = true;
        await context.SaveChangesAsync();

        var list = await svc.GetRecordsList(RecordsFilter.Empty);

        list.Should().NotBeNull();
        list.Any(r => r.Id == delRecord.Id).Should().BeFalse();
        list.Should().HaveCount(19);
    }

    [Test]
    public async Task SoftDeleting_GetRecordsCount_ShouldNotReturnDeletedRecords()
    {
        var context = CreateContext();
        context.SoftDeleting = true;
        Create20Records(context, GetNumberList(20), GetDatesList(20));
        var svc = GetRecordsSearchService(context);
        // mark one record as deleted
        var delRecord = await context.Records.Skip(3).FirstAsync();
        delRecord.Deleted = true;
        await context.SaveChangesAsync();

        int count = await svc.GetRecordsCount(RecordsFilter.Empty);

        count.Should().Be(19);
    }


    [Test]
    public async Task SoftDeleting_GetRecordsList_ShouldNotReturnDeletedImages()
    {
        var context = CreateContext();
        context.SoftDeleting = true;
        Create20Records(context, GetNumberList(20), GetDatesList(20));
        await Add3ImagesForEachRecord(context);
        var svc = GetRecordsSearchService(context);
        // mark two images for different records as deleted
        var (imgId1, imgId2) = await For2RecordsRemoveImagesAndGetThousIds(context);

        var list = await svc.GetRecordsList(RecordsFilter.Empty);

        var allRecordsImages = list.SelectMany(r => r.ImagesRefs);
        allRecordsImages.Should().NotContain(dr => dr.ImageId == imgId1 || dr.ImageId == imgId2, "because images removed from records");
        allRecordsImages.Should().HaveCount(20 * 3 - 2);
    }

    [Test]
    public async Task SoftDeleting_GetRecordsList_ShouldNotReturnDeletedCogitations()
    {
        var context = CreateContext();
        context.SoftDeleting = true;
        Create20Records(context, GetNumberList(20), GetDatesList(20));
        await Add3CogitationsForEachRecord(context);
        var svc = GetRecordsSearchService(context);
        // mark two cogitations for different records as deleted
        var (cId1, cId2) = await For2RecordsRemoveCogitationsAndGetThousIds(context);

        var list = await svc.GetRecordsList(RecordsFilter.Empty);

        var allRecordsThemes = list.SelectMany(r => r.Cogitations);
        allRecordsThemes.Should().NotContain(dr => dr.Id == cId1 || dr.Id == cId2, "because cogitations removed from records");
        allRecordsThemes.Should().HaveCount(20 * 3 - 2);
    }

    [Test]
    public async Task SoftDeleting_GetRecordsList_ShouldNotReturnDeletedThemes()
    {
        var context = CreateContext();
        context.SoftDeleting = true;
        Create20Records(context, GetNumberList(20), GetDatesList(20));
        await Add3ThemesForEachRecord(context);
        var svc = GetRecordsSearchService(context);
        // mark two themes for different records as deleted
        var (thId1, thId2) = await For2RecordsRemoveThemesAndGetThousIds(context);

        var list = await svc.GetRecordsList(RecordsFilter.Empty);

        var allRecordsThemes = list.SelectMany(r => r.ThemesRefs);
        allRecordsThemes.Should().NotContain(dr => dr.ThemeId == thId1 || dr.ThemeId == thId2, "because theme removed from records");
        allRecordsThemes.Should().HaveCount(20 * 3 - 2);
    }

    [Test]
    public async Task GetRecordsList_WithEmptyFilterCriterias_ShouldReturnAllPageRecords()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesList(20));
        var svc = GetRecordsSearchService(context);

        var pageList = await svc.GetRecordsList(RecordsFilter.Empty);

        pageList.Should().HaveCount(20);
        pageList.Select(i => i.Id).Should().BeEquivalentTo(context.Records.Select(r => r.Id).ToList());
    }

    [Test]
    public async Task GetRecordsList_WithEmptyFilterCriterias_And_CombineThemesTrue_ShouldReturnAllPageRecords()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesList(20));
        var svc = GetRecordsSearchService(context);
        var filters = RecordsFilter.Empty;
        filters.CombineThemes = true;

        var pageList = await svc.GetRecordsList(filters);

        pageList.Should().HaveCount(20);
        pageList.Select(i => i.Id).Should().BeEquivalentTo(context.Records.Select(r => r.Id).ToList());
    }

    [Test]
    public async Task GetRecordsCount_WithEmptyFilterCriterias_ShouldReturnAllRecordsCount()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesList(20));
        var svc = GetRecordsSearchService(context);

        int recCount = await svc.GetRecordsCount(RecordsFilter.Empty);

        recCount.Should().Be(context.Records.Count());
    }

    [Test]
    public async Task GetRecordsCount_WithEmptyFilterCriterias_And_CombineThemesTrue_ShouldReturnAllRecordsCount()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesList(20));
        var svc = GetRecordsSearchService(context);
        var filters = RecordsFilter.Empty;
        filters.CombineThemes = true;

        int recCount = await svc.GetRecordsCount(filters);

        recCount.Should().Be(context.Records.Count());
    }

    [Test]
    public async Task GetRecordsList_WithWhiteSpaceInNameFilter_ShouldReturnAllPageRecords()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesList(20));
        var svc = GetRecordsSearchService(context);
        var filterWithWhiteSpace = new RecordsFilter { FilterName = "       " };
        var filterWithNull = new RecordsFilter { FilterName = null };
        var filterWithRmRf = new RecordsFilter { FilterName = "\r\r\n\r\n  \r  \n\n " };

        var pageList1 = await svc.GetRecordsList(filterWithWhiteSpace);
        var pageList2 = await svc.GetRecordsList(filterWithNull);
        var pageList3 = await svc.GetRecordsList(filterWithRmRf);

        pageList1.Should().HaveCount(context.Records.Count());
        pageList1.Should().BeEquivalentTo(
            context.Records.ToList(),
            options => options
                .Including(r => r.Id)
                .Including(r => r.Name));
        pageList2.Should().HaveCount(context.Records.Count());
        pageList2.Should().BeEquivalentTo(
            context.Records.ToList(),
            options => options
                .Including(r => r.Id)
                .Including(r => r.Name));
        pageList3.Should().HaveCount(context.Records.Count());
        pageList3.Should().BeEquivalentTo(
            context.Records.ToList(),
            options => options
                .Including(r => r.Id)
                .Including(r => r.Name));
    }

    [Test]
    public async Task GetRecordsCount_WithWhiteSpaceInNameFilter_ShouldReturnAllRecordsCount()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesList(20));
        var svc = GetRecordsSearchService(context);
        var filterWithWhiteSpace = new RecordsFilter { FilterName = "       " };
        var filterWithNull = new RecordsFilter { FilterName = null };
        var filterWithRmRf = new RecordsFilter { FilterName = "\r\r\n\r\n  \r  \n\n " };

        int recCount1 = await svc.GetRecordsCount(filterWithWhiteSpace);
        int recCount2 = await svc.GetRecordsCount(filterWithNull);
        int recCount3 = await svc.GetRecordsCount(filterWithRmRf);

        recCount1.Should().Be(context.Records.Count());
        recCount2.Should().Be(context.Records.Count());
        recCount3.Should().Be(context.Records.Count());
    }

    [Test]
    public async Task GetRecordsList_WithNameFilter_ShouldReturnAllMatches()
    {
        string searchName = "SearchТекстІї*01";
        const int matches = 4;
        var context = CreateContext();
        Create20Records(context, GetNamesList(), GetDatesList(20));
        var svc = GetRecordsSearchService(context);
        var filters = new RecordsFilter { FilterName = searchName };

        var resPage = await svc.GetRecordsList(filters);

        resPage.Should().HaveCount(matches);
        resPage.Should().SatisfyRespectively(
            first => first.Name.Should().Be("893472983 SearchТекстІї*01"),
            second => second.Name.Should().Be("* /*SearchтекстІї*01/*"),
            third => third.Name.Should().Be("_SearchТекстІї*011АРРОПРОлрффлвыа"),
            fourth => fourth.Name.Should().Be("№%№SearCHТекстІї*01%№!::!;%№:%; "));
    }

    [Test]
    public async Task GetRecordsCount_WithNameFilter_ShouldReturnAllMathchesCount()
    {
        string searchName = "SearchТекстІї*01";
        const int matches = 4;
        var context = CreateContext();
        Create20Records(context, GetNamesList(), GetDatesList(20));
        var svc = GetRecordsSearchService(context);
        var filters = new RecordsFilter { FilterName = searchName };

        int matchesFound = await svc.GetRecordsCount(filters);

        matchesFound.Should().Be(matches);
    }

    [Test]
    public async Task GetRecordsList_ByDate_ForBeforeLastWeek_ShouldReturn13Records()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesList(20));
        var svc = GetRecordsSearchService(context);
        var dateTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-8));
        var filters = new RecordsFilter { ToDate = dateTo };

        var resList = await svc.GetRecordsList(filters);

        resList.Should().OnlyContain(r => r.Date <= dateTo);
    }

    [Test]
    public async Task GetRecordsCount_ByDate_ForBeforeLastWeek_ShouldReturn13()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesList(20));
        var svc = GetRecordsSearchService(context);
        var dateTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        int testCount = 20 - 6; // 20 days except last week (Today date is not added)
        var filters = new RecordsFilter { ToDate = dateTo };

        int count = await svc.GetRecordsCount(filters);

        count.Should().Be(testCount);
    }

    [Test]
    public async Task GetRecordsList_ByDate_ForLastWeek_ShouldReturn6Records()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesList(20));
        var svc = GetRecordsSearchService(context);
        var dateFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var filters = new RecordsFilter { FromDate = dateFrom };

        var resList = await svc.GetRecordsList(filters);

        resList.Should().OnlyContain(r => r.Date >= dateFrom);
    }

    [Test]
    public async Task GetRecordsCount_ByDate_ForLastWeek_ShouldReturn7()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesList(20));
        var svc = GetRecordsSearchService(context);
        var dateFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-6));
        int testCount = 6; // all records for last week, except today
        var filters = new RecordsFilter { FromDate = dateFrom };

        int count = await svc.GetRecordsCount(filters);

        count.Should().Be(testCount);
    }

    [Test]
    public async Task GetRecordsList_ByDate_ForConcreteDate_ShouldReturnWeekAgoRecords()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
        var svc = GetRecordsSearchService(context);
        var concreteDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var filters = new RecordsFilter { FromDate = concreteDate, ToDate = concreteDate };

        var resList = await svc.GetRecordsList(filters);

        resList.Should().NotBeEmpty();
        resList.Should().OnlyContain(rec => rec.Date.Year == concreteDate.Year && rec.Date.Day == concreteDate.Day);
    }

    [Test]
    public async Task GetRecordsCount_ByDate_ForConcreteDate_ShouldReturn2()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
        var svc = GetRecordsSearchService(context);
        var concreteDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        int testCount = 2;
        var filters = new RecordsFilter { FromDate = concreteDate, ToDate = concreteDate };

        int count = await svc.GetRecordsCount(filters);

        count.Should().Be(testCount);
    }

    [Test]
    public async Task GetRecordsList_ByDateAndName_ShouldReturn2Records()
    {
        string searchName = "SearchТекстІї*01";
        var dateFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-14));
        var dateTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-9));
        var context = CreateContext();
        Create20Records(context, GetNamesList(), GetDatesList(20));
        var svc = GetRecordsSearchService(context);
        var filters = new RecordsFilter
        {
            FilterName = searchName,
            FromDate = dateFrom,
            ToDate = dateTo
        };

        var resList = await svc.GetRecordsList(filters);

        resList.Should().OnlyContain(rec =>
            rec.Name.IndexOf(searchName, StringComparison.InvariantCultureIgnoreCase) != -1 &&
            rec.Date >= dateFrom &&
            rec.Date <= dateTo);
    }

    [Test]
    public async Task GetRecordsCount_ByDateAndName_ShouldReturn2()
    {
        string searchName = "SearchТекстІї*01";
        const int matchesInRange = 2;
        var dateFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-14));
        var dateTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-9));
        var context = CreateContext();
        Create20Records(context, GetNamesList(), GetDatesList(20));
        var searchService = GetRecordsSearchService(context);
        var filters = new RecordsFilter
        {
            FilterName = searchName,
            FromDate = dateFrom,
            ToDate = dateTo
        };

        int count = await searchService.GetRecordsCount(filters);

        count.Should().Be(matchesInRange);
    }

    [Test]
    public async Task GetRecordsList_WithNotExistingThemeId_ShouldReturnEmptyList()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
        var searchService = GetRecordsSearchService(context);
        var filter = new RecordsFilter();
        filter.AddThemeId(Guid.NewGuid());

        var list = await searchService.GetRecordsList(filter);

        list.Should().BeEmpty();
    }

    [Test]
    public async Task GetRecordsList_WithNotExistingThemeId_And_CombineThemesTrue_ShouldReturnEmptyList()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
        var searchService = GetRecordsSearchService(context);
        var filter = new RecordsFilter();
        filter.AddThemeId(Guid.NewGuid());
        filter.CombineThemes = true;

        var list = await searchService.GetRecordsList(filter);

        list.Should().BeEmpty();
    }

    [Test]
    public async Task GetRecordsCount_WithNotExistingThemeId_ShouldReturn0()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
        var searchService = GetRecordsSearchService(context);
        var filter = new RecordsFilter();
        filter.AddThemeId(Guid.NewGuid());

        int count = await searchService.GetRecordsCount(filter);

        count.Should().Be(0);
    }

    [Test]
    public async Task GetRecordsCount_WithNotExistingThemeId_And_CombineThemesTrue_ShouldReturn0()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
        var searchService = GetRecordsSearchService(context);
        var filter = new RecordsFilter();
        filter.AddThemeId(Guid.NewGuid());
        filter.CombineThemes = true;

        int count = await searchService.GetRecordsCount(filter);

        count.Should().Be(0);
    }

    [Test]
    public async Task GetRecordsList_ForExistingThemeFilter_ShouldReturnAllMatches()
    {
        var context = CreateContext();
        Create30ThemesAnd20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
        var (themeId1, recordsNamesForTheme1, themeId2, recordsNamesForTheme2) = BindRecordsWithThemes(context, ThemesTestDataSet.ThemesOnly);
        var searchService = GetRecordsSearchService(context);
        var filter = new RecordsFilter();

        filter.AddThemeId(themeId1);
        var result = await searchService.GetRecordsList(filter);

        result.Select(r => r.Name).Should().BeEquivalentTo(recordsNamesForTheme1);

        //----------------------------------------//

        filter.AddThemeId(themeId2);
        filter.RemoveThemeId(themeId1);
        result = await searchService.GetRecordsList(filter);

        result.Select(r => r.Name).Should().BeEquivalentTo(recordsNamesForTheme2);

        //----------------------------------------//

        filter.AddThemeId(themeId1);
        result = await searchService.GetRecordsList(filter);

        result.Select(r => r.Name).Should().BeEquivalentTo(recordsNamesForTheme2.Intersect(recordsNamesForTheme1));
    }

    [Test]
    public async Task GetRecordsList_ForExistingThemeFilter_And_CombineThemesTrue_ShouldReturnAllMatches()
    {
        var context = CreateContext();
        Create30ThemesAnd20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
        var (themeId1, recordsNamesForTheme1, themeId2, recordsNamesForTheme2) = BindRecordsWithThemes(context, ThemesTestDataSet.ThemesOnly);
        var searchService = GetRecordsSearchService(context);
        var filter = new RecordsFilter
        {
            CombineThemes = true
        };

        filter.AddThemeId(themeId1);
        var result = await searchService.GetRecordsList(filter);

        result.Select(r => r.Name).Should().BeEquivalentTo(recordsNamesForTheme1);

        //----------------------------------------//

        filter.AddThemeId(themeId2);
        filter.RemoveThemeId(themeId1);
        result = await searchService.GetRecordsList(filter);

        result.Select(r => r.Name).Should().BeEquivalentTo(recordsNamesForTheme2);

        //----------------------------------------//

        filter.AddThemeId(themeId1);
        result = await searchService.GetRecordsList(filter);

        result.Select(r => r.Name).Should().BeEquivalentTo(recordsNamesForTheme2.Union(recordsNamesForTheme1));
    }

    [Test]
    public async Task GetRecordsCount_ForExistingThemeFilter_ShouldReturnAllMatchesCount()
    {
        var context = CreateContext();
        Create30ThemesAnd20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
        var (themeId1, recordsNamesForTheme1, themeId2, recordsNamesForTheme2) = BindRecordsWithThemes(context, ThemesTestDataSet.ThemesOnly);
        var searchService = GetRecordsSearchService(context);
        var filter = new RecordsFilter();

        filter.AddThemeId(themeId1);
        var result = await searchService.GetRecordsCount(filter);

        result.Should().Be(recordsNamesForTheme1.Count);

        //----------------------------------------//

        filter.AddThemeId(themeId2);
        filter.RemoveThemeId(themeId1);
        result = await searchService.GetRecordsCount(filter);

        result.Should().Be(recordsNamesForTheme2.Count);

        //----------------------------------------//

        filter.AddThemeId(themeId1);
        result = await searchService.GetRecordsCount(filter);

        result.Should().Be(recordsNamesForTheme2.Intersect(recordsNamesForTheme1).Count());
    }

    [Test]
    public async Task GetRecordsCount_ForExistingThemeFilter_And_COmbineThemesTrue_ShouldReturnAllMatchesCount()
    {
        var context = CreateContext();
        Create30ThemesAnd20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
        var (themeId1, recordsNamesForTheme1, themeId2, recordsNamesForTheme2) = BindRecordsWithThemes(context, ThemesTestDataSet.ThemesOnly);
        var searchService = GetRecordsSearchService(context);
        var filter = new RecordsFilter
        {
            CombineThemes = true
        };

        filter.AddThemeId(themeId1);
        var result = await searchService.GetRecordsCount(filter);

        result.Should().Be(recordsNamesForTheme1.Count);

        //----------------------------------------//

        filter.AddThemeId(themeId2);
        filter.RemoveThemeId(themeId1);
        result = await searchService.GetRecordsCount(filter);

        result.Should().Be(recordsNamesForTheme2.Count);

        //----------------------------------------//

        filter.AddThemeId(themeId1);
        result = await searchService.GetRecordsCount(filter);

        result.Should().Be(recordsNamesForTheme2.Union(recordsNamesForTheme1).Count());
    }

    [Test]
    public async Task GetRecordsList_ForDateAndTheme_ShouldReturnOnlyMatchedRecords()
    {
        var context = CreateContext();
        Create30ThemesAnd20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
        var (themeId1, recordsNamesForTheme1, themeId2, recordsNamesForTheme2) = BindRecordsWithThemes(context, ThemesTestDataSet.DatesAndThemesRec);
        var searchService = GetRecordsSearchService(context);
        var dateFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
        var dateTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
        var filter = new RecordsFilter { FromDate = dateFrom, ToDate = dateTo };
        filter.AddThemeId(themeId1);

        var list = context.Records.Where(r => r.Date >= dateFrom && r.Date <= dateTo);

        var result = await searchService.GetRecordsList(filter);

        result.Should().OnlyContain(rec =>
            rec.Date >= dateFrom &&
            rec.Date <= dateTo);
        result.Select(r => r.Name).Should().BeEquivalentTo(new string[] { "05", "07", "09", "10" });

        //-------------------------------------------------//

        filter.AddThemeId(themeId2);
        filter.RemoveThemeId(themeId1);

        result = await searchService.GetRecordsList(filter);

        result.Select(r => r.Name).Should().BeEquivalentTo(new string[] { "05" });
    }

    [Test]
    public async Task GetRecordsCount_ForDateAndTheme_ShouldReturnOnlyMatchedRecordsCount()
    {
        var context = CreateContext();
        Create30ThemesAnd20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
        var (themeId1, _, _, _) = BindRecordsWithThemes(context, ThemesTestDataSet.DatesAndThemesCount);
        var searchService = GetRecordsSearchService(context);
        var dateFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
        var dateTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
        var filter = new RecordsFilter { FromDate = dateFrom, ToDate = dateTo };
        filter.AddThemeId(themeId1);

        int count = await searchService.GetRecordsCount(filter);

        count.Should().Be(3);
    }

    [Test]
    public async Task GetRecordsList_ForNameAndTheme_ShouldReturnOnlyMatchedRecords()
    {
        var context = CreateContext();
        var (themeId1, themeId2, names) = await CreateRecordsWithNamesAndThemes(context);
        var searchService = GetRecordsSearchService(context);
        string searchName = "SearchТекстІї*01";
        var filter = new RecordsFilter { FilterName = searchName };
        filter.AddThemeId(themeId1);

        var result = await searchService.GetRecordsList(filter);

        result.Should().NotBeEmpty();
        result.Should().SatisfyRespectively(
            first => first.Name.Should().Be("56SearchТекстІї*01"),
            second => second.Name.Should().Be("_SearchТекстІї*011АРРОПРОлрффлвыа"));
    }

    [Test]
    public async Task GetRecordsCount_ForNameAndTheme_ShouldReturnOnlyMatchedRecordsCount()
    {
        var context = CreateContext();
        var (themeId1, _, _) = await CreateRecordsWithNamesAndThemes(context);
        var searchService = GetRecordsSearchService(context);
        string searchName = "SearchТекстІї*01";
        var filter = new RecordsFilter { FilterName = searchName };
        filter.AddThemeId(themeId1);

        int count = await searchService.GetRecordsCount(filter);

        count.Should().Be(2);
    }
}
