using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RiseDiary.Common.Search;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RiseDiary.IntegratedTests.Services;

[TestFixture]
internal class RecordsSearchServiceTests : TestFixtureBase
{
    private static CancellationToken CT => CancellationToken.None;

    // ------------------------------------------------------------
    // SOFT DELETE TESTS
    // ------------------------------------------------------------

    [Test]
    public async Task SoftDeleting_GetRecords_ShouldNotReturnDeletedRecords()
    {
        var context = CreateContext();
        context.SoftDeleting = true;
        Create20Records(context, GetNumberList(20), GetDatesList(20));

        var svc = GetRecordsSearchService(context);

        var delRecord = await context.Records.Skip(3).FirstAsync();
        delRecord.Deleted = true;
        await context.SaveChangesAsync();

        var result = await svc.GetRecords(RecordsFilter.Empty, CT);

        result.Succeeded.Should().BeTrue();
        var records = result.Data.records;

        records.Should().NotContain(r => r.Id == delRecord.Id);
        result.Data.count.Should().Be(19);
    }

    [Test]
    public async Task SoftDeleting_GetRecords_ShouldNotReturnDeletedImages()
    {
        var context = CreateContext();
        context.SoftDeleting = true;
        Create20Records(context, GetNumberList(20), GetDatesList(20));
        await Add3ImagesForEachRecord(context);

        var svc = GetRecordsSearchService(context);

        var (imgId1, imgId2) = await For2RecordsRemoveImagesAndGetThousIds(context);

        var result = await svc.GetRecords(RecordsFilter.Empty, CT);

        var images = result.Data.records.SelectMany(r => r.ImagesRefs);

        images.Should().NotContain(i => i.ImageId == imgId1 || i.ImageId == imgId2);
        images.Should().HaveCount(20 * 3 - 2);
    }

    [Test]
    public async Task SoftDeleting_GetRecords_ShouldNotReturnDeletedCogitations()
    {
        var context = CreateContext();
        context.SoftDeleting = true;
        Create20Records(context, GetNumberList(20), GetDatesList(20));
        await Add3CogitationsForEachRecord(context);

        var svc = GetRecordsSearchService(context);

        var (cId1, cId2) = await For2RecordsRemoveCogitationsAndGetThousIds(context);

        var result = await svc.GetRecords(RecordsFilter.Empty, CT);

        var cogitations = result.Data.records.SelectMany(r => r.Cogitations);

        cogitations.Should().NotContain(c => c.Id == cId1 || c.Id == cId2);
        cogitations.Should().HaveCount(20 * 3 - 2);
    }

    [Test]
    public async Task SoftDeleting_GetRecords_ShouldNotReturnDeletedThemes()
    {
        var context = CreateContext();
        context.SoftDeleting = true;
        Create20Records(context, GetNumberList(20), GetDatesList(20));
        await Add3ThemesForEachRecord(context);

        var svc = GetRecordsSearchService(context);

        var (thId1, thId2) = await For2RecordsRemoveThemesAndGetThousIds(context);

        var result = await svc.GetRecords(RecordsFilter.Empty, CT);

        var themes = result.Data.records.SelectMany(r => r.ThemesRefs);

        themes.Should().NotContain(t => t.ThemeId == thId1 || t.ThemeId == thId2);
        themes.Should().HaveCount(20 * 3 - 2);
    }

    // ------------------------------------------------------------
    // EMPTY FILTER TESTS
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecords_WithEmptyFilter_ShouldReturnAllRecords()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesList(20));

        var svc = GetRecordsSearchService(context);

        var result = await svc.GetRecords(RecordsFilter.Empty, CT);

        result.Succeeded.Should().BeTrue();
        result.Data.count.Should().Be(20);
        result.Data.records.Should().HaveCount(20);
    }

    [Test]
    public async Task GetRecords_NameFilterWhitespace_ShouldReturnAll()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesList(20));

        var svc = GetRecordsSearchService(context);

        var filter = new RecordsFilter { FilterName = "   " };

        var result = await svc.GetRecords(filter, CT);

        result.Data.count.Should().Be(20);
        result.Data.records.Should().HaveCount(20);
    }

    // ------------------------------------------------------------
    // NAME FILTER TESTS
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecords_WithNameFilter_ShouldReturnMatches()
    {
        string searchName = "SearchТекстІї*01";
        const int matches = 4;

        var context = CreateContext();
        CreateRecords(context, 20, GetNamesList());

        var svc = GetRecordsSearchService(context);

        var filter = new RecordsFilter { FilterName = searchName };

        var result = await svc.GetRecords(filter, CT);

        result.Succeeded.Should().BeTrue();
        result.Data.records.Should().HaveCount(matches);
    }

    [Test]
    public async Task GetRecords_NameFilter_ShouldHandleUnicode()
    {
        var context = CreateContext();
        CreateRecordsWithUnicodeNames(context);

        var svc = GetRecordsSearchService(context);

        var filter = new RecordsFilter { FilterName = "Її*" };

        var result = await svc.GetRecords(filter, CT);

        result.Data.records.Should().NotBeEmpty();
    }

    // ------------------------------------------------------------
    // DATE FILTER TESTS
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecords_ByDate_ShouldReturnCorrectRange()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20), GetDatesList(20));

        var svc = GetRecordsSearchService(context);

        var dateTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-8));
        var filter = new RecordsFilter { ToDate = dateTo };

        var result = await svc.GetRecords(filter, CT);

        result.Data.records.Should().OnlyContain(r => r.Date <= dateTo);
    }


    // ------------------------------------------------------------
    // THEME FILTER TESTS
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecords_ByTheme_ShouldReturnIntersection()
    {
        var context = CreateContext();
        Create30ThemesAnd20Records(context, GetNumberList(20), GetDatesList(20));

        var (themeId1, recs1, themeId2, recs2) =
            BindRecordsWithThemes(context, ThemesTestDataSet.ThemesOnly);

        var svc = GetRecordsSearchService(context);

        var filter = new RecordsFilter();
        filter.AddThemeId(themeId1);
        filter.AddThemeId(themeId2);

        var result = await svc.GetRecords(filter, CT);

        var expected = recs1.Intersect(recs2);

        result.Data.records.Select(r => r.Name).Should().BeEquivalentTo(expected);
    }

    [Test]
    public async Task GetRecords_CombineThemesTrue_WithEmptyThemes_ShouldReturnAll()
    {
        var context = CreateContext();
        CreateRecords(context, 20);

        var svc = GetRecordsSearchService(context);

        var filter = new RecordsFilter { CombineThemes = true };

        var result = await svc.GetRecords(filter, CT);

        result.Data.count.Should().Be(20);
    }

    // ------------------------------------------------------------
    // DATE + THEME FILTER TESTS
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecords_ByDateAndTheme_ShouldReturnCorrectRecords()
    {
        var context = CreateContext();
        Create30ThemesAnd20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));

        var (themeId1, recs1, themeId2, recs2) =
            BindRecordsWithThemes(context, ThemesTestDataSet.DatesAndThemesRec);

        var svc = GetRecordsSearchService(context);

        var dateFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
        var dateTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));

        var filter = new RecordsFilter { FromDate = dateFrom, ToDate = dateTo };
        filter.AddThemeId(themeId1);

        var result = await svc.GetRecords(filter, CT);

        result.Data.records.Should().OnlyContain(r =>
            r.Date >= dateFrom && r.Date <= dateTo);

        result.Data.records.Select(r => r.Name)
            .Should().BeEquivalentTo(new[] { "05", "07", "09", "10" });
    }

    // ------------------------------------------------------------
    // NAME + THEME FILTER TESTS
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecords_ByNameAndTheme_ShouldReturnCorrectRecords()
    {
        var context = CreateContext();
        var (themeId1, themeId2, names) = await CreateRecordsWithNamesAndThemes(context);

        var svc = GetRecordsSearchService(context);

        string searchName = "SearchТекстІї*01";

        var filter = new RecordsFilter { FilterName = searchName };
        filter.AddThemeId(themeId1);

        var result = await svc.GetRecords(filter, CT);

        result.Data.records.Should().SatisfyRespectively(
            first => first.Name.Should().Be("56SearchТекстІї*01"),
            second => second.Name.Should().Be("_SearchТекстІї*011АРРОПРОлрффлвыа"));
    }

    // ------------------------------------------------------------
    // PAGINATION TESTS
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecords_Pagination_ShouldReturnCorrectPage()
    {
        var context = CreateContext();
        CreateRecords(context, 50);

        var svc = GetRecordsSearchService(context);

        var filter = new RecordsFilter
        {
            PageNo = 1,
            PageSize = 10
        };

        var result = await svc.GetRecords(filter, CT);

        result.Data.records.Should().HaveCount(10);
    }

    // ------------------------------------------------------------
    // SORTING TESTS
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecords_ShouldBeSortedByDateAndCreateDate()
    {
        var context = CreateContext();
        CreateRecords(context, 20);

        var svc = GetRecordsSearchService(context);

        var result = await svc.GetRecords(RecordsFilter.Empty, CT);

        var records = result.Data.records;

        records.Should().BeInDescendingOrder(r => r.Date);
    }

    // ------------------------------------------------------------
    // ERROR HANDLING TESTS
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecords_WhenExceptionThrown_ShouldReturnFailure()
    {
        var context = CreateBrokenContext(); // context that throws
        var svc = GetRecordsSearchService(context);

        var result = await svc.GetRecords(RecordsFilter.Empty, CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().NotBeNullOrEmpty();
    }

    // ------------------------------------------------------------
    // CANCELLATION TESTS
    // ------------------------------------------------------------

    [Test]
    public void GetRecords_WhenCancelled_ShouldThrowOperationCanceled()
    {
        var context = CreateContext();
        CreateRecords(context, 20);

        var svc = GetRecordsSearchService(context);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Func<Task> act = async () => await svc.GetRecords(RecordsFilter.Empty, cts.Token);

        act.Should().ThrowAsync<OperationCanceledException>();
    }
}
