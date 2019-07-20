//using Microsoft.EntityFrameworkCore;
//using NUnit.Framework;
//using RiseDiary.WebUI.Data;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace RiseDiary.IntegratedTests
//{
//    [TestFixture]
//    class ThemeTests : TestFixtureBase
//    {
//        [Test]
//        public async Task AddTheme_ShouldNotThrowException()
//        {
//            var context = CreateContext();
//            int scopeId = Create_Scope(context, "Test Scope");

//            int newRecId = await context.AddTheme(scopeId, @"!@#$%^''""&*()_+,.<><>?//[]||\\апрорпывоаъъЇЇііі.єєєйй");

//            Assert.GreaterOrEqual(newRecId, 1);
//        }

//        [Test]
//        public async Task GetThemesCount_WithoutScopeId_ShouldReturn3()
//        {
//            var context = CreateContext();
//            Create_3Scopes_With1ThemeForEach(context);

//            int recTypesCount = await context.GetThemesCount(null);

//            Assert.AreEqual(3, recTypesCount);
//        }

//        [Test]
//        public async Task GetThemesCount_WithScopeId_ShouldReturnFilteredCount()
//        {
//            var context = CreateContext();

//            int aId1 = Create_Scope(context, "Scope1");
//            int aId2 = Create_Scope(context, "Scope2");
//            Create_ThemesForScope(context, aId1, new List<string> { "theme 1-1", "theme 1-2" });
//            Create_ThemesForScope(context, aId2, new List<string> { "theme 2-1", "theme 2-2", "theme 2-3" });          

//            int themesCount = await context.GetThemesCount(aId1);
//            Assert.AreEqual(2, themesCount);
//            themesCount = await context.GetThemesCount(aId2);
//            Assert.AreEqual(3, themesCount);
//        }

//        [Test]
//        public async Task GetTheme_WithNotExistingId_ShouldReturnNull()
//        {
//            var context = CreateContext();

//            var recType = await context.FetchThemeById(110);

//            Assert.IsNull(recType);
//        }

//        [Test]
//        public async Task FetchTheme_ShouldReturnTheme()
//        {
//            var context = CreateContext();
//            string themeName = @"!@#$%^''""&*()_+,.<><>?//[]||\\апрорпывоаъъЇЇііі.єєєйй";
//            int themeId = Create_Theme(context, themeName);

//            var theme = await context.FetchThemeById(themeId);

//            Assert.IsNotNull(theme);
//            Assert.AreEqual(themeName, theme.ThemeName);
//        }

//        [Test]
//        public async Task UpdateTheme_ShouldUpdateThemeName()
//        {
//            var context = CreateContext();
//            string themeNameOld = @"!@#$%^''""&*()_+,.<><>?//[]||\\апрорпывоаъъЇЇііі.єєєйй";
//            string themeNameNew = @"}{}{P}ЪХЪХъыйыбЙЇіїіїієєєжж.ююббьчсимеуdgfjsjs";
//            int themeId = Create_Theme(context, themeNameOld);
//            var theme = await context.FetchThemeById(themeId);

//            theme.ThemeName = themeNameNew;
//            await context.UpdateTheme(theme);
//            theme = await context.FetchThemeById(themeId);

//            Assert.AreEqual(themeNameNew, theme.ThemeName);
//        }

//        [Test]
//        public Task DeleteTheme_WithNotExistingId_ShouldNotThrowException()
//        {
//            var context = CreateContext();
//            return context.DeleteTheme(1112);
//        }

//        [Test]
//        public async Task DeleteTheme_ShouldDeleteTheme()
//        {
//            var context = CreateContext();
//            int themeId = Create_Theme(context, "Theme Name");

//            await context.DeleteTheme(themeId);
//            var theme = await context.FetchThemeById(themeId);
//            Assert.IsNull(theme);

//            Assert.IsNull(context.Themes.FirstOrDefault(t => t.Id == themeId));
//        }

//        [Test]
//        public async Task GetThemes_ShouldReturnEmptyList()
//        {
//            var context = CreateContext();
//            int scopeId = Create_Scope(context, "1111");

//            var lst = await context.FetchThemesOfScope(null);
//            Assert.IsNotNull(lst);
//            Assert.AreEqual(0, lst.Count);

//            lst = await context.FetchThemesOfScope(scopeId);
//            Assert.IsNotNull(lst);
//            Assert.AreEqual(0, lst.Count);
//        }

//        [Test]
//        public async Task GetThemes_WithoutScopeId_ShouldReturnAllThemes()
//        {
//            var context = CreateContext();
//            Create_3Scopes_With1ThemeForEach(context);

//            var lst = await context.FetchThemesOfScope(null);

//            Assert.IsNotNull(lst);
//            Assert.AreEqual(3, lst.Count);
//        }

//        [Test]
//        public async Task GetThemes_WithScopeId_ShouldReturnFilteredLists()
//        {
//            var context = CreateContext();

//            int aId1 = Create_Scope(context, "1");
//            int aId2 = Create_Scope(context, "2");
//            Create_ThemesForScope(context, aId1, new List<string> { "theme 1-1", "theme 1-2" });
//            Create_ThemesForScope(context, aId2, new List<string> { "theme 2-1", "theme 2-2", "theme 2-3" });

//            var lst = await context.FetchThemesOfScope(aId1);

//            Assert.IsNotNull(lst);
//            Assert.AreEqual(2, lst.Count);

//            lst = await context.FetchThemesOfScope(aId2);

//            Assert.IsNotNull(lst);
//            Assert.AreEqual(3, lst.Count);
//        }

//        [Test]
//        public async Task GetThemesJoined_ShouldReturnJoinedListOfThemesAndScopesNames()
//        {
//            var context = CreateContext();
//            int aId1 = Create_Scope(context, "1");
//            int aId2 = Create_Scope(context, "2");
//            Create_ThemesForScope(context, aId1, new List<string> { "theme 1-1", "theme 1-2" });
//            Create_ThemesForScope(context, aId2, new List<string> { "theme 2-1", "theme 2-2", "theme 2-3" });

//            var joinedList = await context.FetchThemesWithScopes();

//            Assert.IsNotNull(joinedList);
//            Assert.AreEqual(5, joinedList.Count);
//            Assert.AreEqual(2, joinedList.Where(jr => jr.ScopeName == "1").Count());
//            Assert.AreEqual(3, joinedList.Where(jr => jr.ScopeName == "2").Count());
//        }

//        [Test]
//        public async Task FetchThemesIds_WithoutScopeId_ShouldReturnAllThemes()
//        {
//            var context = CreateContext();
//            Create_3Scopes_With1ThemeForEach(context);

//            var lst = await context.FetchThemesIds(null);

//            Assert.IsNotNull(lst);
//            Assert.AreEqual(3, lst.Count);
//        }

//        [Test]
//        public async Task FetchThemesIds_WithScopeId_ShouldReturnFilteredLists()
//        {
//            var context = CreateContext();
//            int aId1 = Create_Scope(context, "1");
//            int aId2 = Create_Scope(context, "2");
//            Create_ThemesForScope(context, aId1, new List<string> { "theme 1-1", "theme 1-2" });
//            Create_ThemesForScope(context, aId2, new List<string> { "theme 2-1", "theme 2-2", "theme 2-3" });

//            var lst = await context.FetchThemesIds(aId1);
//            Assert.IsNotNull(lst);
//            Assert.AreEqual(2, lst.Count);
//            var lst2 = await context.FetchThemesIds(aId2);
//            Assert.IsNotNull(lst2);
//            Assert.AreEqual(3, lst2.Count);
//            Assert.True(lst.TrueForAll(i => !lst2.Contains(i)));
//        }

//        [Test]
//        public async Task BindRecord_RecordTheme_DeleteRecord_ShouldMarkAsDeleted()
//        {
//            var context = CreateContext();
//            int themeId = Create_Theme(context, "Theme Name");
//            int recId = Create_Record(context);
//            await context.AddRecordTheme(recId, themeId);

//            var bindRec = await context.RecordThemes.FirstOrDefaultAsync(br => br.RecordId == recId && br.ThemeId == themeId && !br.Deleted);
//            Assert.IsNotNull(bindRec);

//            await context.DeleteRecord(recId);

//            bindRec = await context.RecordThemes.FirstOrDefaultAsync(br => br.RecordId == recId && br.ThemeId == themeId);
//            Assert.IsNull(bindRec);
//        }

//        [Test]
//        public async Task BindRecord_RecordTheme_DeleteTheme_ShouldMarkAsDeleted()
//        {
//            var context = CreateContext();
//            int themeId = Create_Theme(context, "Theme Name");
//            int recId = Create_Record(context);
//            await context.AddRecordTheme(recId, themeId);

//            var bindRec = await context.RecordThemes.FirstOrDefaultAsync(br => br.RecordId == recId && br.ThemeId == themeId && !br.Deleted);
//            Assert.IsNotNull(bindRec);

//            await context.DeleteTheme(themeId);

//            bindRec = await context.RecordThemes.FirstOrDefaultAsync(br => br.RecordId == recId && br.ThemeId == themeId);
//            Assert.IsNull(bindRec);
//        }

//        [Test]
//        public async Task AddRecordTheme_AfterDeletingTheSame_ShouldUnmarkAsDeleted()
//        {
//            var context = CreateContext();
//            int themeId = Create_Theme(context, "Theme Name");
//            int recId = Create_Record(context);

//            await context.AddRecordTheme(recId, themeId);
//            await context.RemoveRecordTheme(recId, themeId);
//            await context.AddRecordTheme(recId, themeId);

//            int boundRecordCount = context.RecordThemes.Count(br => br.RecordId == recId && br.ThemeId == themeId);
//            Assert.AreEqual(1, boundRecordCount);
//        }
//    }
//}
