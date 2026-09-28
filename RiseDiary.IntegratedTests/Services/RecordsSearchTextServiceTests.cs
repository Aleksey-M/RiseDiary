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
internal class RecordsSearchTextServiceTests : TestFixtureBase
{
    private static CancellationToken CT => CancellationToken.None;

    // ------------------------------------------------------------
    // SOFT DELETE TESTS
    // ------------------------------------------------------------

    [Test]
    public async Task SoftDeleting_GetRecordsList_ShouldNotReturnDeletedRecords()
    {
        var context = CreateContext();
        context.SoftDeleting = true;

        CreateRecords(context, 20, GetNumberList(20, "SearchText_"), GetDatesList(20));

        var svc = GetRecordsSearchTextService(context);

        var delRecord = await context.Records.Skip(3).FirstAsync();
        delRecord.Deleted = true;
        await context.SaveChangesAsync();

        var result = await svc.GetRecordsList(new RecordsTextFilter { SearchText = "SearchText" }, CT);

        result.Succeeded.Should().BeTrue();
        var records = result.Data!.records;

        records.Should().NotContain(r => r.Id == delRecord.Id);
        result.Data!.count.Should().Be(19);
    }

    [Test]
    public async Task SoftDeleting_GetRecordsList_ShouldNotReturnDeletedImages()
    {
        var context = CreateContext();
        context.SoftDeleting = true;

        CreateRecords(context, 20, GetNumberList(20, "SearchText_"), GetDatesList(20));
        await Add3ImagesForEachRecord(context);

        var svc = GetRecordsSearchTextService(context);

        var (imgId1, imgId2) = await For2RecordsRemoveImagesAndGetThousIds(context);

        var result = await svc.GetRecordsList(new RecordsTextFilter { SearchText = "SearchText" }, CT);

        var images = result.Data!.records.SelectMany(r => r.ImagesRefs);

        images.Should().NotContain(i => i.ImageId == imgId1 || i.ImageId == imgId2);
        images.Should().HaveCount(20 * 3 - 2);
    }

    [Test]
    public async Task SoftDeleting_GetRecordsList_ShouldNotReturnDeletedCogitations()
    {
        var context = CreateContext();
        context.SoftDeleting = true;

        CreateRecords(context, 20, GetNumberList(20, "SearchText_"), GetDatesList(20));
        await Add3CogitationsForEachRecord(context);

        var svc = GetRecordsSearchTextService(context);

        var (cId1, cId2) = await For2RecordsRemoveCogitationsAndGetThousIds(context);

        var result = await svc.GetRecordsList(new RecordsTextFilter { SearchText = "SearchText" }, CT);

        var cogitations = result.Data!.records.SelectMany(r => r.Cogitations);

        cogitations.Should().NotContain(c => c.Id == cId1 || c.Id == cId2);
        cogitations.Should().HaveCount(20 * 3 - 2);
    }

    [Test]
    public async Task SoftDeleting_GetRecordsList_ShouldNotReturnDeletedThemes()
    {
        var context = CreateContext();
        context.SoftDeleting = true;

        CreateRecords(context, 20, GetNumberList(20, "SearchText_"), GetDatesList(20));
        await Add3ThemesForEachRecord(context);

        var svc = GetRecordsSearchTextService(context);

        var (thId1, thId2) = await For2RecordsRemoveThemesAndGetThousIds(context);

        var result = await svc.GetRecordsList(new RecordsTextFilter { SearchText = "SearchText" }, CT);

        var themes = result.Data!.records.SelectMany(r => r.ThemesRefs);

        themes.Should().NotContain(t => t.ThemeId == thId1 || t.ThemeId == thId2);
        themes.Should().HaveCount(20 * 3 - 2);
    }

    // ------------------------------------------------------------
    // EMPTY / WHITESPACE SEARCH TEXT
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecordsList_EmptySearchText_ShouldReturnEmpty()
    {
        var context = CreateContext();
        CreateRecords(context, 20, GetNumberList(20, "SearchText_"), GetDatesList(20));

        var svc = GetRecordsSearchTextService(context);

        var result = await svc.GetRecordsList(new RecordsTextFilter { SearchText = "" }, CT);

        result.Succeeded.Should().BeTrue();
        result.Data!.records.Should().BeEmpty();
        result.Data!.count.Should().Be(0);
    }

    [Test]
    public async Task GetRecordsList_WhitespaceSearchText_ShouldReturnEmpty()
    {
        var context = CreateContext();
        CreateRecords(context, 20, GetNumberList(20, "SearchText_"), GetDatesList(20));

        var svc = GetRecordsSearchTextService(context);

        var result = await svc.GetRecordsList(new RecordsTextFilter { SearchText = "   " }, CT);

        result.Succeeded.Should().BeTrue();
        result.Data!.records.Should().BeEmpty();
        result.Data!.count.Should().Be(0);
    }

    // ------------------------------------------------------------
    // NO MATCHES
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecordsList_NoMatches_ShouldReturnEmpty()
    {
        var context = CreateContext();
        CreateRecords(context, 20, GetNumberList(20, "SearchText_"), GetDatesList(20));

        var svc = GetRecordsSearchTextService(context);

        var result = await svc.GetRecordsList(new RecordsTextFilter { SearchText = "UniqueString" }, CT);

        result.Data!.records.Should().BeEmpty();
        result.Data!.count.Should().Be(0);
    }

    // ------------------------------------------------------------
    // MATCHES IN NAME
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecordsList_MatchesInName_ShouldReturnCorrectRecords()
    {
        var context = CreateContext();
        CreateRecords(context, 20, GetNamesList(), GetDatesList(20));

        var svc = GetRecordsSearchTextService(context);

        var result = await svc.GetRecordsList(new RecordsTextFilter { SearchText = SearchSubstring4 }, CT);

        result.Data!.records.Should().HaveCount(4);
        result.Data!.records.Select(r => r.Name)
            .Should().BeEquivalentTo(SearchResult4);
    }

    // ------------------------------------------------------------
    // MATCHES IN TEXT
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecordsList_MatchesInText_ShouldReturnCorrectRecords()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20), GetNamesList().ToList());

        var svc = GetRecordsSearchTextService(context);

        var result = await svc.GetRecordsList(new RecordsTextFilter { SearchText = SearchSubstring4 }, CT);

        result.Data!.records.Should().HaveCount(4);
        result.Data!.records.Select(r => r.Text)
            .Should().BeEquivalentTo(SearchResult4);
    }

    // ------------------------------------------------------------
    // MATCHES IN COGITATIONS
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecordsList_MatchesInCogitations_ShouldReturnCorrectRecords()
    {
        var context = CreateContext();
        Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20));

        await Add3CogitationsForEachRecord(context, "CogitationText_", GetNamesList());

        var svc = GetRecordsSearchTextService(context);

        var result = await svc.GetRecordsList(new RecordsTextFilter { SearchText = SearchSubstring4 }, CT);

        result.Data!.records.Should().HaveCount(4);

        foreach (var r in result.Data!.records)
        {
            r.Cogitations.Any(c => c.Text.Contains(SearchSubstring4, StringComparison.OrdinalIgnoreCase))
                .Should().BeTrue();
        }
    }

    // ------------------------------------------------------------
    // MULTIPLE MATCH SOURCES → DISTINCT RECORDS
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecordsList_SeveralMatches_ShouldReturnDistinctRecords()
    {
        var context = CreateContext();
        Create20Records(context, GetNamesList(), GetDatesList(20), GetNamesList().ToList());

        await Add3CogitationsForEachRecord(context, "CogitationText_");

        var svc = GetRecordsSearchTextService(context);

        var result = await svc.GetRecordsList(new RecordsTextFilter { SearchText = SearchSubstring4 }, CT);

        result.Data!.records.Should().HaveCount(4);
        result.Data!.records.Select(r => r.Text)
            .Should().BeEquivalentTo(SearchResult4);
    }

    // ------------------------------------------------------------
    // PAGINATION
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecordsList_Pagination_ShouldReturnCorrectPage()
    {
        var context = CreateContext();
        CreateRecords(context, 50, GetNumberList(50, "SearchText_"), GetDatesList(50));

        var svc = GetRecordsSearchTextService(context);

        var filter = new RecordsTextFilter
        {
            SearchText = "SearchText",
            PageNo = 1,
            PageSize = 10
        };

        var result = await svc.GetRecordsList(filter, CT);

        result.Data!.records.Should().HaveCount(10);
        result.Data!.count.Should().Be(50);
    }

    // ------------------------------------------------------------
    // SORTING
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecordsList_ShouldBeSortedByDateAndCreateDate()
    {
        var context = CreateContext();
        CreateRecords(context, 20, GetNumberList(20, "SearchText_"), GetDatesList(20));

        var svc = GetRecordsSearchTextService(context);

        var result = await svc.GetRecordsList(new RecordsTextFilter { SearchText = "SearchText" }, CT);

        var records = result.Data!.records;

        records.Should().BeInDescendingOrder(r => r.Date);
    }

    // ------------------------------------------------------------
    // SEARCH TEXT PARSING (SplitSearchText)
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecordsList_SearchTextWithQuotes_ShouldParseCorrectly()
    {
        var context = CreateContext();
        CreateRecords(context, 20, GetNumberList(20, "SearchText_"), GetDatesList(20));

        await Add3CogitationsForEachRecord(context, "CogitationText_");

        var svc = GetRecordsSearchTextService(context);

        var filter = new RecordsTextFilter { SearchText = "\"SearchText_01\"" };

        var result = await svc.GetRecordsList(filter, CT);

        result.Data!.records.Should().HaveCount(1);
        result.Data!.records.First().Name.Should().Contain("SearchText_01");
    }

    [Test]
    public async Task GetRecordsList_SearchTextWithMultiplePartsAndFields_ShouldReturn()
    {
        var context = CreateContext();
        CreateRecords(context, 20, GetNumberList(20, "SearchText_"), GetDatesList(20));

        await Add3CogitationsForEachRecord(context, "CogitationText_");

        var svc = GetRecordsSearchTextService(context);

        var filter = new RecordsTextFilter { SearchText = "\"CogitationText_02\" 'SearchText_07'" };

        var result = await svc.GetRecordsList(filter, CT);

        result.Data!.records.Should().HaveCount(1);
        result.Data!.records.First().Name.Should().Contain("SearchText_07");
    }

    [Test]
    public async Task GetRecordsList_SearchTextWithMultipleParts_ShouldReturnUnion()
    {
        var context = CreateContext();
        CreateRecords(context, 20, GetNumberList(20, "SearchText_"), GetDatesList(20));

        var svc = GetRecordsSearchTextService(context);

        var filter = new RecordsTextFilter { SearchText = "'SearchText_01' \"SearchText_02\"" };

        var result = await svc.GetRecordsList(filter, CT);

        result.Data!.records.Should().HaveCount(2);
    }

    // ------------------------------------------------------------
    // ERROR HANDLING
    // ------------------------------------------------------------

    [Test]
    public async Task GetRecordsList_WhenExceptionThrown_ShouldReturnFailure()
    {
        var context = CreateBrokenContext();
        var svc = GetRecordsSearchTextService(context);

        var result = await svc.GetRecordsList(new RecordsTextFilter { SearchText = "SearchText" }, CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().NotBeNullOrEmpty();
    }

    // ------------------------------------------------------------
    // CANCELLATION
    // ------------------------------------------------------------

    [Test]
    public void GetRecordsList_WhenCancelled_ShouldThrowOperationCanceled()
    {
        var context = CreateContext();
        CreateRecords(context, 20);

        var svc = GetRecordsSearchTextService(context);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Func<Task> act = async () =>
            await svc.GetRecordsList(new RecordsTextFilter { SearchText = "SearchText" }, cts.Token);

        act.Should().ThrowAsync<OperationCanceledException>();
    }
}
