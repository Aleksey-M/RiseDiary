using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace RiseDiary.IntegratedTests.Services
{
    [TestFixture]
    internal class ScopesServiceTests : TestedServices
    {
        [Test]
        public async Task AddScope_ShouldNotThrowException()
        {
            var svc = GetScopesService();

            Guid newId = await svc.AddScope(@"New Scope ""!@#$%^''""&*()_+,.<><>?//[]||\\апрорпывоаъъЇЇііі.єєєйй");

            newId.Should().NotBeEmpty();
        }

        [Test]
        public async Task AddSeveralScopes_ShouldReturnDifferentIds()
        {
            var svc = GetScopesService();

            Guid id1 = await svc.AddScope("New Scope 1");
            Guid id2 = await svc.AddScope("New Scope 2");
            Guid id3 = await svc.AddScope("New Scope 3");

            id1.Should().NotBe(id2).And.NotBe(id3);
            id2.Should().NotBe(id3);
        }

        [Test]
        public void AddScope_WithNullParameter_ShouldThrowArgumentException()
        {
            var svc = GetScopesService();
            Func<Task> action = async () => _ = await svc.AddScope(null!);

            action.Should().ThrowAsync<ArgumentException>();
        }

        [Test]
        public void AddScope_WithEmptyParameter_ShouldThrowArgumentException()
        {
            var svc = GetScopesService();
            Func<Task> action1 = async () => _ = await svc.AddScope("");
            Func<Task> action2 = async () => _ = await svc.AddScope(" ");
            Func<Task> action3 = async () => _ = await svc.AddScope("   ");

            action1.Should().ThrowAsync<ArgumentException>();
            action2.Should().ThrowAsync<ArgumentException>();
            action3.Should().ThrowAsync<ArgumentException>();
        }

        [Test]
        public async Task GetScope_WithNotExistingId_ShouldReturnNull()
        {
            var svc = GetScopesService();

            var scope = await svc.FetchScopeById(Guid.NewGuid());

            scope.Should().BeNull();
        }

        [Test]
        public async Task GetScope_ShouldReturnScope()
        {
            var svc = GetScopesService();
            var ScopesData = new Dictionary<string, Guid>
            {
                { @"""_)(*&^%$#@!фівраХЇЇїіййєєє", await svc.AddScope(@"""_)(*&^%$#@!фівраХЇЇїіййєєє") },
                { @"...^&*(::;[]", await svc.AddScope(@"...^&*(::;[]") },
                { @"'|||\\//", await svc.AddScope(@"'|||\\//") }
            };

            foreach (KeyValuePair<string, Guid> pair in ScopesData)
            {
                pair.Key.Should().Be((await svc.FetchScopeById(pair.Value)).ScopeName);
            }
        }

        [Test]
        public async Task GetScopes_ShouldReturnEmptyList()
        {
            var svc = GetScopesService();

            var list = await svc.GetScopes(themesActuality: true);

            list.Should().NotBeNull();
            list.Should().BeEmpty();

            list = await svc.GetScopes(themesActuality: false);

            list.Should().NotBeNull();
            list.Should().BeEmpty();
        }

        [Test]
        public async Task GetScopes_All_ShouldReturnListWith3Scopes()
        {
            var svc = GetScopesService();
            await svc.AddScope("New Scope 1");
            await svc.AddScope("New Scope 2");
            await svc.AddScope("New Scope 3");

            var list = await svc.GetScopes(themesActuality: null);

            list.Should().NotBeNull();
            list.Should().HaveCount(3);
        }

        [Test]
        public async Task GetScopes_AllActive_Or_NotActive_ShouldReturnEmptyList()
        {
            var svc = GetScopesService();
            await svc.AddScope("New Scope 1");
            await svc.AddScope("New Scope 2");
            await svc.AddScope("New Scope 3");

            var list = await svc.GetScopes(themesActuality: true);

            list.Should().NotBeNull();
            list.Should().BeEmpty();

            list = await svc.GetScopes(themesActuality: false);

            list.Should().NotBeNull();
            list.Should().BeEmpty();
        }

        [Test]
        public async Task GetScopes_NotActive_ShouldReturnListWith2Scopes()
        {
            var svc = GetScopesService();
            var id1 = await svc.AddScope("New Scope 1");
            await svc.AddScope("New Scope 2");
            var id3 = await svc.AddScope("New Scope 3");
            var id4 = await svc.AddScope("New Scope 4");
            var tid1 = await svc.AddTheme(id1, "Theme 1", false);
            await svc.AddTheme(id3, "Theme 2", true);
            var tid2 = await svc.AddTheme(id3, "Theme 3", false);
            await svc.AddTheme(id4, "Theme 4", true);

            var list = await svc.GetScopes(themesActuality: false);

            list.Should().NotBeNull();
            list.Should().HaveCount(2);
            list.Should().SatisfyRespectively(
                s =>
                {
                    s.Id.Should().Be(id1);
                    s.Themes.Should().HaveCount(1);
                    s.Themes.First().Id.Should().Be(tid1);
                },
                s =>
                {
                    s.Id.Should().Be(id3);
                    s.Themes.Should().HaveCount(1);
                    s.Themes.First().Id.Should().Be(tid2);
                });
        }

        [Test]
        public async Task GetScopes_Active_ShouldReturnListWith2Scopes()
        {
            var svc = GetScopesService();
            var id1 = await svc.AddScope("New Scope 1");
            await svc.AddScope("New Scope 2");
            var id3 = await svc.AddScope("New Scope 3");
            var id4 = await svc.AddScope("New Scope 4");
            await svc.AddTheme(id1, "Theme 1", false);
            var tid1 = await svc.AddTheme(id3, "Theme 2", true);
            await svc.AddTheme(id3, "Theme 3", false);
            var tid2 = await svc.AddTheme(id4, "Theme 4", true);

            var list = await svc.GetScopes(themesActuality: true);

            list.Should().NotBeNull();
            list.Should().HaveCount(2);
            list.Should().SatisfyRespectively(
                s =>
                {
                    s.Id.Should().Be(id3);
                    s.Themes.Should().HaveCount(1);
                    s.Themes.First().Id.Should().Be(tid1);
                },
                s =>
                {
                    s.Id.Should().Be(id4);
                    s.Themes.Should().HaveCount(1);
                    s.Themes.First().Id.Should().Be(tid2);
                });
        }

        [Test]
        public async Task GetScopes_OnlyActive_ShouldReturnEmptyList()
        {
            var svc = GetScopesService();
            await svc.AddScope("New Scope 1");
            await svc.AddScope("New Scope 2");
            await svc.AddScope("New Scope 3");

            var list = await svc.GetScopes(themesActuality: true);

            list.Should().NotBeNull();
            list.Should().BeEmpty();
        }

        [Test]
        public async Task GetScopes_OnlyActive_ShouldReturnListWith1Scope()
        {
            var svc = GetScopesService();
            var id1 = await svc.AddScope("New Scope 1");
            await svc.AddTheme(id1, "Not actual theme", false);
            await svc.AddScope("New Scope 2");
            var id2 = await svc.AddScope("New Scope 3");
            await svc.AddTheme(id2, "Actual theme", true);

            var list = await svc.GetScopes(themesActuality: true);

            list.Should().NotBeNull();
            list.Should().HaveCount(1);
            list.Should().SatisfyRespectively(s =>
            {
                s.Id.Should().Be(id2);
                s.ScopeName.Should().Be("New Scope 3");
            });
        }

        [Test]
        public async Task UpdateScope_ShouldUpdateScopeName()
        {
            var svc = GetScopesService();
            Guid id = await svc.AddScope("New Scope 1");

            await svc.UpdateScopeName(id, @"""'''[]!@#$%^&*()_+::"":;;<><>,.");
            var updatedScope = await svc.FetchScopeById(id);

            updatedScope.ScopeName.Should().Be(@"""'''[]!@#$%^&*()_+::"":;;<><>,.");
        }

        [Test]
        public async Task CanDeleteScope_ShouldReturnTrue()
        {
            var svc = GetScopesService();
            Guid id = await svc.AddScope("New Scope 1");

            (await svc.CanDeleteScope(id)).Should().BeTrue();
        }

        [Test]
        public async Task CanDeleteScope_WithDeletedThemes_ShouldReturnTrue()
        {
            var svc = GetScopesService();
            Guid scopeId = await svc.AddScope("New Scope 1");
            Guid typeId = await svc.AddTheme(scopeId, "New type", false);

            await svc.DeleteTheme(typeId);

            (await svc.CanDeleteScope(scopeId)).Should().BeTrue();
        }

        [Test]
        public async Task CanDeleteScope_ShouldReturnFalse()
        {
            var svc = GetScopesService();

            Guid scopeId = await svc.AddScope("New Scope 1");
            Guid typeId = await svc.AddTheme(scopeId, "New type", false);

            (await svc.CanDeleteScope(scopeId)).Should().BeFalse();
            typeId.Should().NotBeEmpty();
        }

        [Test]
        public Task DeleteScope_WithNotExistingId_ShouldNotThrowException()
        {
            var svc = GetScopesService();
            return svc.DeleteScope(Guid.NewGuid());
        }

        [Test]
        public async Task DeleteScope_ShouldDeleteScope()
        {
            var context = CreateContext();
            var svc = GetScopesService(context);
            Guid id = await svc.AddScope("New Scope 1");

            await svc.DeleteScope(id);
            var scope = await svc.FetchScopeById(id);

            scope.Should().BeNull();
            context.Scopes.IgnoreQueryFilters().SingleOrDefault(s => s.Id == id).Should().NotBeNull();
        }

        [Test]
        public async Task DeleteScope_WithDeletedThemes_ShouldDeleteScope()
        {
            var context = CreateContext();
            var svc = GetScopesService(context);
            var themeId = Create_Theme(context, "Theme Name");
            var scopeId = (await context.Themes.FindAsync(themeId)).ScopeId;

            await svc.DeleteTheme(themeId);
            await svc.DeleteScope(scopeId);
            var scope = await svc.FetchScopeById(scopeId);

            scope.Should().BeNull();
            context.Scopes.IgnoreQueryFilters().SingleOrDefault(s => s.Id == scopeId).Should().NotBeNull();
        }

        [Test]
        public async Task AddTheme_ShouldNotThrowException()
        {
            var context = CreateContext();
            var svc = GetScopesService(context);
            var scopeId = Create_Scope(context, "Test Scope");

            var newRecId = await svc.AddTheme(scopeId, @"!@#$%^''""&*()_+,.<><>?//[]||\\апрорпывоаъъЇЇііі.єєєйй", false);

            newRecId.Should().NotBeEmpty();
        }

        [Test]
        public async Task FetchTheme_ShouldReturnTheme()
        {
            var context = CreateContext();
            string themeName = @"!@#$%^''""&*()_+,.<><>?//[]||\\апрорпывоаъъЇЇііі.єєєйй";
            var (scopeId, themeId) = CreateThemeWithScope(context, themeName);
            var svc = GetScopesService(context);

            var scope = await svc.FetchScopeById(scopeId);
            var theme = scope.Themes.SingleOrDefault(t => t.Id == themeId);

            theme.Should().NotBeNull();
            theme?.ThemeName.Should().Be(themeName);
        }

        [Test]
        public async Task UpdateTheme_ShouldUpdateThemeName()
        {
            var context = CreateContext();
            string themeNameOld = @"!@#$%^''""&*()_+,.<><>?//[]||\\апрорпывоаъъЇЇііі.єєєйй";
            string themeNameNew = @"}{}{P}ЪХЪХъыйыбЙЇіїіїієєєжж.ююббьчсимеуdgfjsjs";
            var (scopeId, themeId) = CreateThemeWithScope(context, themeNameOld);
            var svc = GetScopesService(context);
            var scope = await svc.FetchScopeById(scopeId);
            var theme = scope.Themes.SingleOrDefault(t => t.Id == themeId);

            await svc.UpdateTheme(themeId, themeNameNew, false);
            scope = await svc.FetchScopeById(scopeId);
            theme = scope.Themes.Single(t => t.Id == themeId);

            theme.ThemeName.Should().Be(themeNameNew);
        }

        [Test]
        public Task DeleteTheme_WithNotExistingId_ShouldNotThrowException()
        {
            var svc = GetScopesService();
            return svc.DeleteTheme(Guid.NewGuid());
        }

        [Test]
        public async Task DeleteTheme_ShouldDeleteTheme()
        {
            var context = CreateContext();
            var (scopeId, themeId) = CreateThemeWithScope(context, "Theme Name");
            var svc = GetScopesService(context);

            await svc.DeleteTheme(themeId);

            var scope = await svc.FetchScopeById(scopeId);
            var theme = scope.Themes.SingleOrDefault(t => t.Id == themeId);

            theme.Should().BeNull();
            context.Themes.IgnoreQueryFilters().SingleOrDefault(t => t.Id == themeId).Should().NotBeNull();
        }

        [Test]
        public async Task ThemeActuality_ChangeTwice()
        {
            var context = CreateContext();
            var scopes = Create_3Scopes_With1ThemeForEach(context);
            var themes = scopes.SelectMany(s => s.Themes).ToList();
            var svc = GetScopesService(context);

            await svc.UpdateTheme(themes[0].Id, themeActuality: true);
            await svc.UpdateTheme(themes[1].Id, themeActuality: false);
            await svc.UpdateTheme(themes[2].Id, themeActuality: true);

            context.Themes.Single(t => t.Id == themes[0].Id).Actual.Should().BeTrue();
            context.Themes.Single(t => t.Id == themes[1].Id).Actual.Should().BeFalse();
            context.Themes.Single(t => t.Id == themes[2].Id).Actual.Should().BeTrue();

            await svc.UpdateTheme(themes[2].Id, themeActuality: false);

            context.Themes.Single(t => t.Id == themes[0].Id).Actual.Should().BeTrue();
            context.Themes.Single(t => t.Id == themes[1].Id).Actual.Should().BeFalse();
            context.Themes.Single(t => t.Id == themes[2].Id).Actual.Should().BeFalse();
        }

        [Test]
        public async Task DeleteTheme_WithEnabledSD_ShouldMarkAsDeleted()
        {
            var context = CreateContext();
            context.SoftDeleting = true;
            var svc = GetScopesService(context);
            (_, var scope, _) = CreateEntities(context);
            var theme = scope.Themes.First();

            await svc.DeleteTheme(theme.Id);

            var t = await context.Themes.FindAsync(theme.Id);
            t.Should().NotBeNull();
            t.Deleted.Should().BeTrue();

            var rt = context.RecordThemes.IgnoreQueryFilters().FirstOrDefault(recTheme => recTheme.ThemeId == theme.Id);
            rt.Should().NotBeNull();
            rt?.Deleted.Should().BeTrue();
        }

        [Test]
        public async Task DeleteTheme_WithDisabledSD_ShouldDeleteRecords()
        {
            var context = CreateContext();
            context.SoftDeleting = false;
            var svc = GetScopesService(context);
            (_, var scope, _) = CreateEntities(context);
            var theme = scope.Themes.First();

            await svc.DeleteTheme(theme.Id);

            var t = await context.Themes.FindAsync(theme.Id);
            t.Should().BeNull();

            var rt = context.RecordThemes.IgnoreQueryFilters().FirstOrDefault(recTheme => recTheme.ThemeId == theme.Id);
            rt.Should().BeNull();
        }

        [Test]
        public async Task DeleteScope_WithEnabledSD_ShouldMarkAsDeleted()
        {
            var context = CreateContext();
            context.SoftDeleting = true;
            var svc = GetScopesService(context);
            Guid id = await svc.AddScope("New Scope 1");

            await svc.DeleteScope(id);

            var scope = await context.Scopes.FindAsync(id);
            scope.Should().NotBeNull();
            scope.Deleted.Should().BeTrue();
        }

        [Test]
        public async Task DeleteScope_WithDisabledSD_ShouldDeleteRecords()
        {
            var context = CreateContext();
            context.SoftDeleting = false;
            var svc = GetScopesService(context);
            Guid id = await svc.AddScope("New Scope 1");

            await svc.DeleteScope(id);

            var scope = await context.Scopes.FindAsync(id);
            scope.Should().BeNull();
        }
    }
}
