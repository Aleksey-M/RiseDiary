//using NUnit.Framework;
//using System.Threading.Tasks;
//using RiseDiary.WebUI.Data;
//using System.Linq;
//using Microsoft.EntityFrameworkCore;

//namespace RiseDiary.IntegratedTests
//{
//    [TestFixture]
//    internal class SoftDeletingTests : TestFixtureBase
//    {
//        [Test]
//        public async Task DeleteRecord_WithEnabledSD_ShouldMarkAsDeleted()
//        {
//            var context = CreateContext();
//            context.SoftDeleting = true;
//            (var rec, _, _) = CreateEntities(context);

//            await context.DeleteRecord(rec.Id);

//            var r = await context.Records.FindAsync(rec.Id);
//            Assert.IsNotNull(r);
//            Assert.True(r.Deleted);

//            var c = context.Cogitations.IgnoreQueryFilters().FirstOrDefault(cog => cog.RecordId == rec.Id);
//            Assert.IsNotNull(c);
//            Assert.True(c.Deleted);

//            var rt = context.RecordThemes.IgnoreQueryFilters().FirstOrDefault(recTheme => recTheme.RecordId == rec.Id);
//            Assert.IsNotNull(rt);
//            Assert.True(rt.Deleted);

//            var ri = context.RecordImages.IgnoreQueryFilters().FirstOrDefault(recImage => recImage.RecordId == rec.Id);
//            Assert.IsNotNull(ri);
//            Assert.True(ri.Deleted);
//        }

//        [Test]
//        public async Task  DeleteRecord_WithDisabledSD_ShouldDeleteRecords()
//        {
//            var context = CreateContext();
//            context.SoftDeleting = false;
//            (var rec, _, _) = CreateEntities(context);

//            await context.DeleteRecord(rec.Id);

//            var r = await context.Records.FindAsync(rec.Id);
//            Assert.IsNull(r);

//            var c = context.Cogitations.FirstOrDefault(cog => cog.RecordId == rec.Id);
//            Assert.IsNull(c);

//            var rt = context.RecordThemes.FirstOrDefault(recTheme => recTheme.RecordId == rec.Id);
//            Assert.IsNull(rt);

//            var ri = context.RecordImages.FirstOrDefault(recImage => recImage.RecordId == rec.Id);
//            Assert.IsNull(ri);
//        }
        
//        [Test]
//        public async Task  DeleteImage_WithEnabledSD_ShouldMarkAsDeleted()
//        {
//            var context = CreateContext();
//            context.SoftDeleting = true;
//            (_, _, var image) = CreateEntities(context);

//            await context.DeleteImage(image.Id);

//            var i = await context.Images.FindAsync(image.Id);
//            Assert.IsNotNull(i);
//            Assert.True(i.Deleted);

//            var ri = context.RecordImages.IgnoreQueryFilters().FirstOrDefault(recImage => recImage.ImageId == image.Id);
//            Assert.IsNotNull(ri);
//            Assert.True(ri.Deleted);
//        }

//        [Test]
//        public async Task  DeleteImage_WithDisabledSD_ShouldDeleteRecords()
//        {
//            var context = CreateContext();
//            context.SoftDeleting = false;
//            (_, _, var image) = CreateEntities(context);

//            await context.DeleteImage(image.Id);

//            var i = await context.Images.FindAsync(image.Id);
//            Assert.IsNull(i);

//            var ri = context.RecordImages.IgnoreQueryFilters().FirstOrDefault(recImage => recImage.ImageId == image.Id);
//            Assert.IsNull(ri);
//        }
        
//        [Test]
//        public async Task  DeleteTheme_WithEnabledSD_ShouldMarkAsDeleted()
//        {
//            var context = CreateContext();
//            context.SoftDeleting = true;
//            (_, var scope, _) = CreateEntities(context);
//            var theme = scope.Themes.First();

//            await context.DeleteTheme(theme.Id);

//            var t = await context.Themes.FindAsync(theme.Id);
//            Assert.IsNotNull(t);
//            Assert.True(t.Deleted);

//            var rt = context.RecordThemes.IgnoreQueryFilters().FirstOrDefault(recTheme => recTheme.ThemeId == theme.Id);
//            Assert.IsNotNull(rt);
//            Assert.True(rt.Deleted);
//        }

//        [Test]
//        public async Task  DeleteTheme_WithDisabledSD_ShouldDeleteRecords()
//        {
//            var context = CreateContext();
//            context.SoftDeleting = false;
//            (_, var scope, _) = CreateEntities(context);
//            var theme = scope.Themes.First();

//            await context.DeleteTheme(theme.Id);

//            var t = await context.Themes.FindAsync(theme.Id);
//            Assert.IsNull(t);

//            var rt = context.RecordThemes.IgnoreQueryFilters().FirstOrDefault(recTheme => recTheme.ThemeId == theme.Id);
//            Assert.IsNull(rt);
//        }
        
//        [Test]
//        public async Task DeleteScope_WithEnabledSD_ShouldMarkAsDeleted()
//        {
//            var context = CreateContext();
//            context.SoftDeleting = true;
//            int id = await context.AddScope("New Scope 1");

//            await context.DeleteScope(id);

//            var scope = await context.Scopes.FindAsync(id);
//            Assert.IsNotNull(scope);
//            Assert.True(scope.Deleted);
//        }

//        [Test]
//        public async Task DeleteScope_WithDisabledSD_ShouldDeleteRecords()
//        {
//            var context = CreateContext();
//            context.SoftDeleting = false;
//            int id = await context.AddScope("New Scope 1");

//            await context.DeleteScope(id);

//            var scope = await context.Scopes.FindAsync(id);
//            Assert.IsNull(scope);
//        }
        
//        [Test]
//        public async Task DeleteCogitation_WithEnabledSD_ShouldMarkAsDeleted()
//        {
//            var context = CreateContext();
//            context.SoftDeleting = true;
//            var (recId, cogId) = Create_3Records_1Cogitation(context);

//            await context.DeleteCogitation(cogId);

//            var cogitation = await context.Cogitations.FindAsync(cogId);
//            Assert.IsNotNull(cogitation);
//            Assert.True(cogitation.Deleted);
//        }

//        [Test]
//        public async Task DeleteCogitation_WithDisabledSD_ShouldDeleteRecords()
//        {
//            var context = CreateContext();
//            context.SoftDeleting = false;
//            var (recId, cogId) = Create_3Records_1Cogitation(context);

//            await context.DeleteCogitation(cogId);

//            var cogitation = await context.Cogitations.FindAsync(cogId);
//            Assert.IsNull(cogitation);
//        }
        
//        [Test]
//        public async Task  RebindRecordWithTheme_WithEnabledSD_ShouldRebind()
//        {
//            var context = CreateContext();
//            context.SoftDeleting = true;
//            (var rec, var scope, _) = CreateEntities(context);
//            var theme = scope.Themes.First();

//            var recTheme = await context.RecordThemes.FirstOrDefaultAsync(rt => rt.RecordId == rec.Id && rt.ThemeId == theme.Id);
//            Assert.NotNull(recTheme);

//            await context.RemoveRecordTheme(rec.Id, theme.Id);

//            recTheme = await context.RecordThemes.FirstOrDefaultAsync(rt => rt.RecordId == rec.Id && rt.ThemeId == theme.Id);
//            Assert.Null(recTheme);

//            await context.AddRecordTheme(rec.Id, theme.Id);
//            recTheme = await context.RecordThemes.FirstOrDefaultAsync(rt => rt.RecordId == rec.Id && rt.ThemeId == theme.Id);
//            Assert.NotNull(recTheme);
//        }

//        [Test]
//        public async Task  RebindRecordWithTheme_WithDisabledSD_ShouldRebind()
//        {
//            var context = CreateContext();
//            context.SoftDeleting = false;
//            (var rec, var scope, _) = CreateEntities(context);
//            var theme = scope.Themes.First();

//            var recTheme = await context.RecordThemes.FirstOrDefaultAsync(rt => rt.RecordId == rec.Id && rt.ThemeId == theme.Id);
//            Assert.NotNull(recTheme);

//            await context.RemoveRecordTheme(rec.Id, theme.Id);

//            recTheme = await context.RecordThemes.FirstOrDefaultAsync(rt => rt.RecordId == rec.Id && rt.ThemeId == theme.Id);
//            Assert.Null(recTheme);

//            await context.AddRecordTheme(rec.Id, theme.Id);
//            recTheme = await context.RecordThemes.FirstOrDefaultAsync(rt => rt.RecordId == rec.Id && rt.ThemeId == theme.Id);
//            Assert.NotNull(recTheme);
//        }

//        [Test]
//        public async Task  RebindRecordWithImage_WithEnabledSD_ShouldRebind()
//        {
//            var context = CreateContext();
//            context.SoftDeleting = true;
//            (var rec, _, var image) = CreateEntities(context);

//            var recImage = await context.RecordImages.FirstOrDefaultAsync(ri => ri.RecordId == rec.Id && ri.ImageId == image.Id);
//            Assert.NotNull(recImage);

//            await context.RemoveRecordImage(rec.Id, image.Id);

//            recImage = await context.RecordImages.FirstOrDefaultAsync(ri => ri.RecordId == rec.Id && ri.ImageId == image.Id);
//            Assert.Null(recImage);

//            await context.AddRecordImage(rec.Id, image.Id);
//            recImage = await context.RecordImages.FirstOrDefaultAsync(ri => ri.RecordId == rec.Id && ri.ImageId == image.Id);
//            Assert.NotNull(recImage);
//        }

//        [Test]
//        public async Task  RebindRecordWithImage_WithDisabledSD_ShouldRebind()
//        {
//            var context = CreateContext();
//            context.SoftDeleting = false;
//            (var rec, _, var image) = CreateEntities(context);

//            var recImage = await context.RecordImages.FirstOrDefaultAsync(ri => ri.RecordId == rec.Id && ri.ImageId == image.Id);
//            Assert.NotNull(recImage);

//            await context.RemoveRecordImage(rec.Id, image.Id);

//            recImage = await context.RecordImages.FirstOrDefaultAsync(ri => ri.RecordId == rec.Id && ri.ImageId == image.Id);
//            Assert.Null(recImage);

//            await context.AddRecordImage(rec.Id, image.Id);
//            recImage = await context.RecordImages.FirstOrDefaultAsync(ri => ri.RecordId == rec.Id && ri.ImageId == image.Id);
//            Assert.NotNull(recImage);
//        }            
//    }
//}
