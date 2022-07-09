using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.IntegratedTests.Services
{
    internal class RecordsThemesServiceTests : TestFixtureBase
    {
        [Test]
        public async Task BindRecord_RecordTheme_DeleteRecord_ShouldMarkAsDeleted()
        {
            var context = CreateContext();
            var svc = GetRecordsThemesService(context);
            var recSvc = GetRecordsService(context);
            var themeId = new Guid[] { CreateTheme(context, "Theme Name") };
            var recId = CreateRecord(context);
            await svc.AddRecordTheme(recId, themeId);

            var bindRec = await context.RecordThemes.SingleOrDefaultAsync(br => br.RecordId == recId && br.ThemeId == themeId[0] && !br.Deleted);
            bindRec.Should().NotBeNull();

            await recSvc.DeleteRecord(recId);

            bindRec = await context.RecordThemes.SingleOrDefaultAsync(br => br.RecordId == recId && br.ThemeId == themeId[0]);
            bindRec.Should().BeNull();
        }

        [Test]
        public async Task BindRecord_RecordTheme_DeleteTheme_ShouldMarkAsDeleted()
        {
            var context = CreateContext();
            var svc = GetRecordsThemesService(context);
            var themesSvc = GetScopesService(context);
            var themeId = CreateTheme(context, "Theme Name");
            var recId = CreateRecord(context);
            await svc.AddRecordTheme(recId, new Guid[] { themeId });

            var bindRec = await context.RecordThemes.SingleOrDefaultAsync(br => br.RecordId == recId && br.ThemeId == themeId && !br.Deleted);
            bindRec.Should().NotBeNull();

            await themesSvc.DeleteTheme(themeId);

            bindRec = await context.RecordThemes.SingleOrDefaultAsync(br => br.RecordId == recId && br.ThemeId == themeId);
            bindRec.Should().BeNull();
        }

        [Test]
        public async Task AddRecordTheme_AfterDeletingTheSame_ShouldUnmarkAsDeleted()
        {
            var context = CreateContext();
            var svc = GetRecordsThemesService(context);
            var themeId = new Guid[] { CreateTheme(context, "Theme Name") };
            var recId = CreateRecord(context);

            await svc.AddRecordTheme(recId, themeId);
            await svc.RemoveRecordTheme(recId, themeId);
            await svc.AddRecordTheme(recId, themeId);

            int boundRecordCount = context.RecordThemes.Count(br => br.RecordId == recId && br.ThemeId == themeId[0]);
            boundRecordCount.Should().Be(1);
        }

        [Test]
        public async Task RebindRecordWithTheme_WithEnabledSD_ShouldRebind()
        {
            var context = CreateContext();
            var svc = GetRecordsThemesService(context);
            context.SoftDeleting = true;
            (var rec, var scope, _) = CreateEntities(context);
            var theme = new Guid[] { scope.Themes.First().Id };

            var recTheme = await context.RecordThemes.FirstOrDefaultAsync(rt => rt.RecordId == rec.Id && rt.ThemeId == theme[0]);
            recTheme.Should().NotBeNull();

            await svc.RemoveRecordTheme(rec.Id, theme);

            recTheme = await context.RecordThemes.FirstOrDefaultAsync(rt => rt.RecordId == rec.Id && rt.ThemeId == theme[0]);
            recTheme.Should().BeNull();

            await svc.AddRecordTheme(rec.Id, theme);
            recTheme = await context.RecordThemes.FirstOrDefaultAsync(rt => rt.RecordId == rec.Id && rt.ThemeId == theme[0]);
            recTheme.Should().NotBeNull();
        }

        [Test]
        public async Task RebindRecordWithTheme_WithDisabledSD_ShouldRebind()
        {
            var context = CreateContext();
            var svc = GetRecordsThemesService(context);
            context.SoftDeleting = false;
            (var rec, var scope, _) = CreateEntities(context);
            var theme = new Guid[] { scope.Themes.First().Id };

            var recTheme = await context.RecordThemes.FirstOrDefaultAsync(rt => rt.RecordId == rec.Id && rt.ThemeId == theme[0]);
            recTheme.Should().NotBeNull();

            await svc.RemoveRecordTheme(rec.Id, theme);

            recTheme = await context.RecordThemes.FirstOrDefaultAsync(rt => rt.RecordId == rec.Id && rt.ThemeId == theme[0]);
            recTheme.Should().BeNull();

            await svc.AddRecordTheme(rec.Id, theme);
            recTheme = await context.RecordThemes.FirstOrDefaultAsync(rt => rt.RecordId == rec.Id && rt.ThemeId == theme[0]);
            recTheme.Should().NotBeNull();
        }

        [Test]
        public async Task GetRecordThemes_ShouldReturnEmptyList()
        {
            var context = CreateContext();
            var recordId = CreateRecord(context);
            var svc = GetRecordsThemesService(context);

            var list = await svc.GetRecordThemes(recordId);

            list.Should().NotBeNull();
            list.Should().BeEmpty();
        }

        [Test]
        public async Task GetRecordThemes_ShouldReturn3Themes()
        {
            var context = CreateContext();
            var recordId = CreateRecord(context);
            var svc = GetRecordsThemesService(context);
            await Add3ThemesForEachRecord(context);

            var list = await svc.GetRecordThemes(recordId);

            list.Should().NotBeNull();
            list.Should().HaveCount(3);
            list.Should().AllSatisfy(
                x =>
                {
                    x.Theme.Should().NotBeNull();
                });
        }
    }
}
