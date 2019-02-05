using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RiseDiary.WebUI.Data;
using RiseDiary.Model;
using System.Linq;

namespace RiseDiary.SqliteStorages.IntegratedTests
{
    [TestFixture]
    internal class ScopeTests : TestFixtureBase
    {
        [Test]
        public async Task AddScope_ShouldNotThrowException()
        {
            var context =  CreateContext();

            int newId = await context.AddScope(@"New Scope ""!@#$%^''""&*()_+,.<><>?//[]||\\апрорпывоаъъЇЇііі.єєєйй");

            Assert.GreaterOrEqual(newId, 1);
        }

        [Test]
        public async Task AddSeveralScopes_ShouldReturnDifferentIds()
        {
            var context =  CreateContext();

            int Id1 = await context.AddScope("New Scope 1");
            int Id2 = await context.AddScope("New Scope 2");
            int Id3 = await context.AddScope("New Scope 3");

            Assert.AreNotEqual(Id1, Id2);
            Assert.AreNotEqual(Id1, Id3);
            Assert.AreNotEqual(Id3, Id2);
        }

        [Test]
        public void AddScope_WithNullParameter_ShouldThrowArgumentException()
        {
            var context =  CreateContext();

            int id;
            Assert.ThrowsAsync<ArgumentException>(async () => id = await context.AddScope(null));
        }

        [Test]
        public void AddScope_WithEmptyParameter_ShouldThrowArgumentException()
        {
            var context =  CreateContext();

            int id;
            Assert.ThrowsAsync<ArgumentException>(async () => id = await context.AddScope(""));
            Assert.ThrowsAsync<ArgumentException>(async () => id = await context.AddScope(" "));
            Assert.ThrowsAsync<ArgumentException>(async () => id = await context.AddScope("   "));
        }

        [Test]
        public async Task GetScopesCount_ShouldReturnZero()
        {
            var context =  CreateContext();

            int count = await context.GetScopesCount();

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task GetScopesCount_ShouldReturn3()
        {
            var context =  CreateContext();

            await context.AddScope("New Scope 1");
            await context.AddScope("New Scope 2");
            await context.AddScope("New Scope 3");
            int count = await context.GetScopesCount();

            Assert.AreEqual(3, count);
        }

        [Test]
        public async Task GetScope_WithNotExistingId_ShouldReturnNull()
        {
            var context =  CreateContext();

            var scope = await context.FetchScopeById(100);

            Assert.IsNull(scope);
        }

        [Test]
        public async Task GetScope_ShouldReturnScope()
        {
            var context =  CreateContext();
            var ScopesData = new Dictionary<string, int>
            {
                { @"""_)(*&^%$#@!фівраХЇЇїіййєєє", await context.AddScope(@"""_)(*&^%$#@!фівраХЇЇїіййєєє") },
                { @"...^&*(::;[]", await context.AddScope(@"...^&*(::;[]") },
                { @"'|||\\//", await context.AddScope(@"'|||\\//") }
            };

            foreach (KeyValuePair<string, int> pair in ScopesData)
            {
                Assert.AreEqual(pair.Key, (await context.FetchScopeById(pair.Value)).ScopeName);
            }
        }

        [Test]
        public async Task GetScopes_ShouldReturnEmptyList()
        {
            var context =  CreateContext();

            var list = await context.FetchAllScopes();

            Assert.NotNull(list);
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public async Task GetScopes_ShouldReturnListWith3Scopes()
        {
            var context =  CreateContext();
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
            var context =  CreateContext();
            var Scope = new DiaryScope { Id = 7, ScopeName = "not existing Scope" };

            int id;
            Assert.ThrowsAsync<ArgumentException>(async () => id = await context.UpdateScope(Scope));
        }

        [Test]
        public async Task UpdateScope_ShouldUpdateScopeName()
        {
            var context =  CreateContext();
            int id = await context.AddScope("New Scope 1");
            var Scope = await context.FetchScopeById(id);

            Scope.ScopeName = @"""'''[]!@#$%^&*()_+::"":;;<><>,.";
            int id2 = await context.UpdateScope(Scope);
            var updatedScope = await context.FetchScopeById(id2);

            Assert.AreEqual(id, id2);
            Assert.AreEqual(Scope.ScopeName, updatedScope.ScopeName);
        }

        [Test]
        public async Task CanDeleteScope_ShouldReturnTrue()
        {
            var context =  CreateContext();
            int id = await context.AddScope("New Scope 1");

            Assert.IsTrue(await context.CanDeleteScope(id));
        }

        [Test]
        public async Task CanDeleteScope_WithDeletedThemes_ShouldReturnTrue()
        {
            var context = CreateContext();
            int ScopeId = await context.AddScope("New Scope 1");
            int typeId = await context.AddTheme(ScopeId, "New type");

            await context.DeleteTheme(typeId);

            Assert.IsTrue(await context.CanDeleteScope(ScopeId));
        }

        [Test]
        public async Task CanDeleteScope_ShouldReturnFalse()
        {
            var context =  CreateContext();
            
            int ScopeId = await context.AddScope("New Scope 1");
            int typeId = await context.AddTheme(ScopeId, "New type");

            Assert.IsFalse(await context.CanDeleteScope(ScopeId));
        }

        [Test]
        public async Task DeleteScope_WithNotExistingId_ShouldNotThrowException()
        {
            var context =  CreateContext();
            await context.DeleteScope(150);
        }

        [Test]
        public async Task DeleteScope_ShouldDeleteScope()
        {
            var context =  CreateContext();
            int id = await context.AddScope("New Scope 1");

            await context.DeleteScope(id);
            var Scope = await context.FetchScopeById(id);

            Assert.IsNull(Scope);

            Assert.IsNotNull(context.Scopes.FirstOrDefault(s => s.Id == id && s.Deleted));
        }
    }
}
