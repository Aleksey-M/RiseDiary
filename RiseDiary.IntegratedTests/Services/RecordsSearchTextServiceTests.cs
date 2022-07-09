using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RiseDiary.IntegratedTests.Stubs;
using RiseDiary.Model;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.IntegratedTests.Services
{
    [TestFixture]
    internal class RecordsSearchTextServiceTests : TestFixtureBase
    {
        [Test]
        public async Task SoftDeleting_GetRecordsListByText_ShouldNotReturnDeletedRecords()
        {
            var context = CreateContext();
            context.SoftDeleting = true;
            Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20));
            var recSearchByTextSvc = GetRecordsSearchTextService(context);
            // mark one record as deleted
            var delRecord = await context.Records.Skip(3).FirstAsync();
            delRecord.Deleted = true;
            await context.SaveChangesAsync();

            var list = await recSearchByTextSvc.GetRecordsList(new RecordsTextFilter { SearchText = "SearchText" });

            list.Should().NotBeNull();
            list.Any(r => r.Id == delRecord.Id).Should().BeFalse();
            list.Should().HaveCount(19);
        }

        [Test]
        public async Task SoftDeleting_GetRecordsCount_ShouldNotReturnDeletedRecords()
        {
            var context = CreateContext();
            context.SoftDeleting = true;
            Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20));
            var recSearchByTextSvc = GetRecordsSearchTextService(context);
            // mark one record as deleted
            var delRecord = await context.Records.Skip(3).FirstAsync();
            delRecord.Deleted = true;
            await context.SaveChangesAsync();

            int count = await recSearchByTextSvc.GetRecordsCount("SearchText");

            count.Should().Be(19);
        }


        [Test]
        public async Task SoftDeleting_GetRecordsListByText_ShouldNotReturnDeletedImages()
        {
            var context = CreateContext();
            context.SoftDeleting = true;
            Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20));
            await Add3ImagesForEachRecord(context);
            var recSearchByTextSvc = GetRecordsSearchTextService(context);
            // mark two images for different records as deleted
            var (imgId1, imgId2) = await For2RecordsRemoveImagesAndGetThousIds(context);

            var list = await recSearchByTextSvc.GetRecordsList(new RecordsTextFilter { SearchText = "SearchText" });

            var allRecordsImages = list.SelectMany(r => r.ImagesRefs);
            allRecordsImages.Should().NotContain(dr => dr.ImageId == imgId1 || dr.ImageId == imgId2, "because images removed from records");
            allRecordsImages.Should().HaveCount(20 * 3 - 2);
        }

        [Test]
        public async Task SoftDeleting_GetRecordsListByText_ShouldNotReturnDeletedCogitations()
        {
            var context = CreateContext();
            context.SoftDeleting = true;
            Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20));
            await Add3CogitationsForEachRecord(context);
            var recSearchByTextSvc = GetRecordsSearchTextService(context);
            // mark two cogitations for different records as deleted
            var (cId1, cId2) = await For2RecordsRemoveCogitationsAndGetThousIds(context);

            var list = await recSearchByTextSvc.GetRecordsList(new RecordsTextFilter { SearchText = "SearchText" });

            var allRecordsThemes = list.SelectMany(r => r.Cogitations);
            allRecordsThemes.Should().NotContain(dr => dr.Id == cId1 || dr.Id == cId2, "because cogitations removed from records");
            allRecordsThemes.Should().HaveCount(20 * 3 - 2);
        }

        [Test]
        public async Task SoftDeleting_GetRecordsListByText_ShouldNotReturnDeletedThemes()
        {
            var context = CreateContext();
            context.SoftDeleting = true;
            Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20));
            await Add3ThemesForEachRecord(context);
            var recSearchByTextSvc = GetRecordsSearchTextService(context);
            // mark two themes for different records as deleted
            var (thId1, thId2) = await For2RecordsRemoveThemesAndGetThousIds(context);

            var list = await recSearchByTextSvc.GetRecordsList(new RecordsTextFilter { SearchText = "SearchText" });

            var allRecordsThemes = list.SelectMany(r => r.ThemesRefs);
            allRecordsThemes.Should().NotContain(dr => dr.ThemeId == thId1 || dr.ThemeId == thId2, "because theme removed from records");
            allRecordsThemes.Should().HaveCount(20 * 3 - 2);
        }

        [Test]
        public async Task GetRecordsListByText_ShouldReplacePlaceholdereByHostAndPort_InRecordName()
        {
            var context = CreateContext();
            context.SoftDeleting = true;
            Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20));
            var recSearchByTextSvc = GetRecordsSearchTextService(context);
            var hostAndPortService = new HostAndPortStub();
            // add links
            var (rec1, rec2) = await For2RecordsAddLinkToName(context, hostAndPortService);

            var list = await recSearchByTextSvc.GetRecordsList(new RecordsTextFilter { SearchText = "SearchText" });

            var r1 = list.SingleOrDefault(r => r.Id == rec1);
            r1?.Name.Should().NotContain(hostAndPortService.GetHostAndPortPlaceholder());
            r1?.Name.Should().Contain(hostAndPortService.GetHostAndPort());
            var r2 = list.SingleOrDefault(r => r.Id == rec2);
            r2?.Name.Should().NotContain(hostAndPortService.GetHostAndPortPlaceholder());
            r2?.Name.Should().Contain(hostAndPortService.GetHostAndPort());
        }

        [Test]
        public async Task GetRecordsListByText_ShouldReplacePlaceholdereByHostAndPort_InRecordText()
        {
            var context = CreateContext();
            context.SoftDeleting = true;
            Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20));
            var recSearchByTextSvc = GetRecordsSearchTextService(context);
            var hostAndPortService = new HostAndPortStub();
            // add links
            var (rec1, rec2) = await For2RecordsAddLinkToText(context, hostAndPortService);

            var list = await recSearchByTextSvc.GetRecordsList(new RecordsTextFilter { SearchText = "SearchText" });

            var r1 = list.SingleOrDefault(r => r.Id == rec1);
            r1?.Text.Should().NotContain(hostAndPortService.GetHostAndPortPlaceholder());
            r1?.Text.Should().Contain(hostAndPortService.GetHostAndPort());
            var r2 = list.SingleOrDefault(r => r.Id == rec2);
            r2?.Text.Should().NotContain(hostAndPortService.GetHostAndPortPlaceholder());
            r2?.Text.Should().Contain(hostAndPortService.GetHostAndPort());
        }

        [Test]
        public async Task GetRecordsListByText_ShouldReplacePlaceholdereByHostAndPort_InCogitationsText()
        {
            var context = CreateContext();
            context.SoftDeleting = true;
            Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20));
            await Add3CogitationsForEachRecord(context);
            var recSearchByTextSvc = GetRecordsSearchTextService(context);
            var hostAndPortService = new HostAndPortStub();
            // add links
            var (cog1, cog2) = await For2RecordsAddLinkToCogitations(context, hostAndPortService);

            var list = await recSearchByTextSvc.GetRecordsList(new RecordsTextFilter { SearchText = "SearchText" });

            var allCogitations = list.SelectMany(r => r.Cogitations).ToList();
            var c1 = allCogitations.SingleOrDefault(c => c.Id == cog1);
            c1?.Text.Should().NotContain(hostAndPortService.GetHostAndPortPlaceholder());
            c1?.Text.Should().Contain(hostAndPortService.GetHostAndPort());
            var c2 = list.SingleOrDefault(r => r.Id == cog2);
            c2?.Text.Should().NotContain(hostAndPortService.GetHostAndPortPlaceholder());
            c2?.Text.Should().Contain(hostAndPortService.GetHostAndPort());
        }

        [Test]
        public async Task GetRecordsListByText_ByEmptyString_ShouldReturnEmptyList()
        {
            var context = CreateContext();
            Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20));
            await Add3CogitationsForEachRecord(context, "CogitationText_");
            var recSearchByTextSvc = GetRecordsSearchTextService(context);
            var filter = new RecordsTextFilter { SearchText = "" };

            var list = await recSearchByTextSvc.GetRecordsList(filter);

            list.Should().NotBeNull();
            list.Should().BeEmpty();
        }

        [Test]
        public async Task GetRecordsCount_ByEmptyString_ShouldReturn0()
        {
            var context = CreateContext();
            Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20));
            await Add3CogitationsForEachRecord(context, "CogitationText_");
            var recSearchByTextSvc = GetRecordsSearchTextService(context);

            int count = await recSearchByTextSvc.GetRecordsCount("");

            count.Should().Be(0);
        }

        [Test]
        public async Task GetRecordsListByText_ByWhitespaces_ShouldReturnEmptyList()
        {
            var context = CreateContext();
            Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20));
            await Add3CogitationsForEachRecord(context, "CogitationText_");
            var recSearchByTextSvc = GetRecordsSearchTextService(context);
            var filter = new RecordsTextFilter { SearchText = "    " };

            var list = await recSearchByTextSvc.GetRecordsList(filter);

            list.Should().NotBeNull();
            list.Should().BeEmpty();
        }

        [Test]
        public async Task GetRecordsCount_ByWhitespaces_ShouldReturn0()
        {
            var context = CreateContext();
            Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20));
            await Add3CogitationsForEachRecord(context, "CogitationText_");
            var recSearchByTextSvc = GetRecordsSearchTextService(context);

            int count = await recSearchByTextSvc.GetRecordsCount(" ");

            count.Should().Be(0);
        }

        [Test]
        public async Task GetRecordsListByText_WithoutMatches_ShouldReturnEmptyList()
        {
            var context = CreateContext();
            Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20));
            await Add3CogitationsForEachRecord(context, "CogitationText_");
            var recSearchByTextSvc = GetRecordsSearchTextService(context);
            var filter = new RecordsTextFilter { SearchText = "UniqueString" };

            var list = await recSearchByTextSvc.GetRecordsList(filter);

            list.Should().NotBeNull();
            list.Should().BeEmpty();
        }

        [Test]
        public async Task GetRecordsCount_WithoutMatches_ShouldReturn0()
        {
            var context = CreateContext();
            Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20));
            await Add3CogitationsForEachRecord(context, "CogitationText_");
            var recSearchByTextSvc = GetRecordsSearchTextService(context);

            int count = await recSearchByTextSvc.GetRecordsCount("UniqueString");

            count.Should().Be(0);
        }

        [Test]
        public async Task GetRecordsListByText_MatchesInName_ShouldReturn4Records()
        {
            var context = CreateContext();
            Create20Records(context, GetNamesList(), GetDatesList(20));
            await Add3CogitationsForEachRecord(context, "CogitationText_");
            var recSearchByTextSvc = GetRecordsSearchTextService(context);
            var filter = new RecordsTextFilter { SearchText = SearchSubstring4 };

            var list = await recSearchByTextSvc.GetRecordsList(filter);

            list.Should().NotBeNull();
            list.Should().HaveCount(4);
            list.Should().SatisfyRespectively(
                r1 => r1.Name.Should().Be(SearchResult4.ElementAt(0)),
                r2 => r2.Name.Should().Be(SearchResult4.ElementAt(1)),
                r3 => r3.Name.Should().Be(SearchResult4.ElementAt(2)),
                r4 => r4.Name.Should().Be(SearchResult4.ElementAt(3)));
        }

        [Test]
        public async Task GetRecordsCount_MatchesInName_ShouldReturn4()
        {
            var context = CreateContext();
            Create20Records(context, GetNamesList(), GetDatesList(20));
            await Add3CogitationsForEachRecord(context, "CogitationText_");
            var recSearchByTextSvc = GetRecordsSearchTextService(context);

            int count = await recSearchByTextSvc.GetRecordsCount(SearchSubstring4);

            count.Should().Be(4);
        }

        [Test]
        public async Task GetRecordsListByText_MatchesInText_ShouldReturn4Records()
        {
            var context = CreateContext();
            Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20), GetNamesList().ToList());
            await Add3CogitationsForEachRecord(context, "CogitationText_");
            var recSearchByTextSvc = GetRecordsSearchTextService(context);
            var filter = new RecordsTextFilter { SearchText = SearchSubstring4 };

            var list = await recSearchByTextSvc.GetRecordsList(filter);

            list.Should().NotBeNull();
            list.Should().HaveCount(4);
            list.Should().SatisfyRespectively(
                r1 => r1.Text.Should().Be(SearchResult4.ElementAt(0)),
                r2 => r2.Text.Should().Be(SearchResult4.ElementAt(1)),
                r3 => r3.Text.Should().Be(SearchResult4.ElementAt(2)),
                r4 => r4.Text.Should().Be(SearchResult4.ElementAt(3)));
        }

        [Test]
        public async Task GetRecordsCount_MatchesInText_ShouldReturn4()
        {
            var context = CreateContext();
            Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20), GetNamesList().ToList());
            await Add3CogitationsForEachRecord(context, "CogitationText_");
            var recSearchByTextSvc = GetRecordsSearchTextService(context);

            int count = await recSearchByTextSvc.GetRecordsCount(SearchSubstring4);

            count.Should().Be(4);
        }

        [Test]
        public async Task GetRecordsListByText_MatchesInCogitationText_ShouldReturn4Records()
        {
            var context = CreateContext();
            Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20));
            await Add3CogitationsForEachRecord(context, "CogitationText_", GetNamesList());
            var recSearchByTextSvc = GetRecordsSearchTextService(context);
            var filter = new RecordsTextFilter { SearchText = SearchSubstring4 };

            var list = await recSearchByTextSvc.GetRecordsList(filter);

            list.Should().NotBeNull();
            list.Should().HaveCount(4);
            foreach (var r in list)
            {
                r.Cogitations.Should().NotBeEmpty();
                r.Cogitations.Any(c => c.Text.ToUpper().Contains(SearchSubstring4.ToUpper())).Should().BeTrue();
            }
        }

        [Test]
        public async Task GetRecordsCount_MatchesInCogitationText_ShouldReturn4()
        {
            var context = CreateContext();
            Create20Records(context, GetNumberList(20, "SearchText_"), GetDatesList(20));
            await Add3CogitationsForEachRecord(context, "CogitationText_", GetNamesList());
            var recSearchByTextSvc = GetRecordsSearchTextService(context);

            int count = await recSearchByTextSvc.GetRecordsCount(SearchSubstring4);

            count.Should().Be(4);
        }

        [Test]
        public async Task GetRecordsListByText_SeveralMatches_ShouldReturnDistinctRecords()
        {
            var context = CreateContext();
            Create20Records(context, GetNamesList(), GetDatesList(20), GetNamesList().ToList());
            await Add3CogitationsForEachRecord(context, "CogitationText_");
            var recSearchByTextSvc = GetRecordsSearchTextService(context);
            var filter = new RecordsTextFilter { SearchText = SearchSubstring4 };

            var list = await recSearchByTextSvc.GetRecordsList(filter);

            list.Should().NotBeNull();
            list.Should().HaveCount(4);
            list.Should().SatisfyRespectively(
                r1 => r1.Text.Should().Be(SearchResult4.ElementAt(0)),
                r2 => r2.Text.Should().Be(SearchResult4.ElementAt(1)),
                r3 => r3.Text.Should().Be(SearchResult4.ElementAt(2)),
                r4 => r4.Text.Should().Be(SearchResult4.ElementAt(3)));
        }

        [Test]
        public async Task GetRecordsCount_SeveralMatches_ShouldReturn4()
        {
            var context = CreateContext();
            Create20Records(context, GetNamesList(), GetDatesList(20), GetNamesList().ToList());
            await Add3CogitationsForEachRecord(context, "CogitationText_");
            var recSearchByTextSvc = GetRecordsSearchTextService(context);

            int count = await recSearchByTextSvc.GetRecordsCount(SearchSubstring4);

            count.Should().Be(4);
        }
    }
}
