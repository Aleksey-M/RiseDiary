using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RiseDiary.WebUI.Data;
using RiseDiary.Model;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace RiseDiary.IntegratedTests
{
    [TestFixture]
    internal class ScopeTests : TestFixtureBase
    {
        [Test]
        public async Task AddScope_ShouldNotThrowException()
        {
            var context = CreateContext();

            Guid newId = await context.AddScope(@"New Scope ""!@#$%^''""&*()_+,.<><>?//[]||\\апрорпывоаъъЇЇііі.єєєйй");

            Assert.AreNotEqual(default, newId);
        }

        [Test]
        public async Task AddSeveralScopes_ShouldReturnDifferentIds()
        {
            var context = CreateContext();

            Guid Id1 = await context.AddScope("New Scope 1");
            Guid Id2 = await context.AddScope("New Scope 2");
            Guid Id3 = await context.AddScope("New Scope 3");

            Assert.AreNotEqual(Id1, Id2);
            Assert.AreNotEqual(Id1, Id3);
            Assert.AreNotEqual(Id3, Id2);
        }

        [Test]
        public void AddScope_WithNullParameter_ShouldThrowArgumentException()
        {
            var context = CreateContext();

            Guid id;
            Assert.ThrowsAsync<ArgumentException>(async () => id = await context.AddScope(null));
        }

        [Test]
        public void AddScope_WithEmptyParameter_ShouldThrowArgumentException()
        {
            var context = CreateContext();

            Guid id;
            Assert.ThrowsAsync<ArgumentException>(async () => id = await context.AddScope(""));
            Assert.ThrowsAsync<ArgumentException>(async () => id = await context.AddScope(" "));
            Assert.ThrowsAsync<ArgumentException>(async () => id = await context.AddScope("   "));
        }

        [Test]
        public async Task GetScopesCount_ShouldReturnZero()
        {
            var context = CreateContext();

            int count = await context.GetScopesCount();

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task GetScopesCount_ShouldReturn3()
        {
            var context = CreateContext();

            await context.AddScope("New Scope 1");
            await context.AddScope("New Scope 2");
            await context.AddScope("New Scope 3");
            int count = await context.GetScopesCount();

            Assert.AreEqual(3, count);
        }

        [Test]
        public async Task GetScope_WithNotExistingId_ShouldReturnNull()
        {
            var context = CreateContext();

            var scope = await context.FetchScopeById(Guid.NewGuid());

            Assert.IsNull(scope);
        }

        [Test]
        public async Task GetScope_ShouldReturnScope()
        {
            var context = CreateContext();
            var ScopesData = new Dictionary<string, Guid>
            {
                { @"""_)(*&^%$#@!фівраХЇЇїіййєєє", await context.AddScope(@"""_)(*&^%$#@!фівраХЇЇїіййєєє") },
                { @"...^&*(::;[]", await context.AddScope(@"...^&*(::;[]") },
                { @"'|||\\//", await context.AddScope(@"'|||\\//") }
            };

            foreach (KeyValuePair<string, Guid> pair in ScopesData)
            {
                Assert.AreEqual(pair.Key, (await context.FetchScopeById(pair.Value)).ScopeName);
            }
        }

        [Test]
        public async Task GetScopes_ShouldReturnEmptyList()
        {
            var context = CreateContext();

            var list = await context.FetchAllScopes();

            Assert.NotNull(list);
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public async Task GetScopes_ShouldReturnListWith3Scopes()
        {
            var context = CreateContext();
            await context.AddScope("New Scope 1");
            await context.AddScope("New Scope 2");
            await context.AddScope("New Scope 3");

            var list = await context.FetchAllScopes();

            Assert.NotNull(list);
            Assert.AreEqual(3, list.Count);
        }

        [Test]
        public void UpdateScope_WithNotExistingId_ShouldThrowArgumentException()
        {
            var context = CreateContext();
            var Scope = new DiaryScope { Id = Guid.NewGuid(), ScopeName = "not existing Scope" };

            Guid id;
            Assert.ThrowsAsync<ArgumentException>(async () => id = await context.UpdateScope(Scope));
        }

        [Test]
        public async Task UpdateScope_ShouldUpdateScopeName()
        {
            var context = CreateContext();
            Guid id = await context.AddScope("New Scope 1");
            var Scope = await context.FetchScopeById(id);

            Scope.ScopeName = @"""'''[]!@#$%^&*()_+::"":;;<><>,.";
            Guid id2 = await context.UpdateScope(Scope);
            var updatedScope = await context.FetchScopeById(id2);

            Assert.AreEqual(id, id2);
            Assert.AreEqual(Scope.ScopeName, updatedScope.ScopeName);
        }

        [Test]
        public async Task CanDeleteScope_ShouldReturnTrue()
        {
            var context = CreateContext();
            Guid id = await context.AddScope("New Scope 1");

            Assert.IsTrue(await context.CanDeleteScope(id));
        }

        [Test]
        public async Task CanDeleteScope_WithDeletedThemes_ShouldReturnTrue()
        {
            var context = CreateContext();
            Guid ScopeId = await context.AddScope("New Scope 1");
            Guid typeId = await context.AddTheme(ScopeId, "New type");

            await context.DeleteTheme(typeId);

            Assert.IsTrue(await context.CanDeleteScope(ScopeId));
        }

        [Test]
        public async Task CanDeleteScope_ShouldReturnFalse()
        {
            var context = CreateContext();

            Guid ScopeId = await context.AddScope("New Scope 1");
            Guid typeId = await context.AddTheme(ScopeId, "New type");

            Assert.IsFalse(await context.CanDeleteScope(ScopeId));
            Assert.AreNotEqual(default, typeId);
        }

        [Test]
        public Task DeleteScope_WithNotExistingId_ShouldNotThrowException()
        {
            var context = CreateContext();
            return context.DeleteScope(Guid.NewGuid());
        }

        [Test]
        public async Task DeleteScope_ShouldDeleteScope()
        {
            var context = CreateContext();
            Guid id = await context.AddScope("New Scope 1");

            await context.DeleteScope(id);
            var Scope = await context.FetchScopeById(id);

            Assert.IsNull(Scope);

            Assert.IsNotNull(context.Scopes.IgnoreQueryFilters().FirstOrDefault(s => s.Id == id));
        }

        [Test]
        public async Task DeleteScope_WithDeletedThemes_ShouldDeleteScope()
        {
            var context = CreateContext();
            var themeId = Create_Theme(context, "Theme Name");
            var scopeId = (await context.Themes.FindAsync(themeId)).ScopeId;

            await context.DeleteTheme(themeId);
            await context.DeleteScope(scopeId);
            var Scope = await context.FetchScopeById(scopeId);

            Assert.IsNull(Scope);

            Assert.IsNotNull(context.Scopes.IgnoreQueryFilters().FirstOrDefault(s => s.Id == scopeId));
        }
    }
}
