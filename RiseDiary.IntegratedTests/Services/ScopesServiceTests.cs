using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RiseDiary.WebAPI.Application.Scopes.Services.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.IntegratedTests.Services;

[TestFixture]
internal class ScopesServiceTests : TestFixtureBase
{
    [Test]
    public async Task CreateScope_ShouldNotThrowException()
    {
        var svc = GetScopesService();

        var result = await svc.Apply(new CreateScope
        {
            Name = @"New Scope ""!@#$%^''""&*()_+,.<><>?//[]||\\апрорпывоаъъЇЇііі.єєєйй",
            Description = null
        });

        result.Succeeded.Should().BeTrue();
        result.Data!.NewEntityId.Should().NotBeEmpty();
    }

    [Test]
    public async Task CreateSeveralScopes_ShouldReturnDifferentIds()
    {
        var svc = GetScopesService();

        var id1 = (await svc.Apply(new CreateScope { Name = "New Scope 1" })).Data!.NewEntityId;
        var id2 = (await svc.Apply(new CreateScope { Name = "New Scope 2" })).Data!.NewEntityId;
        var id3 = (await svc.Apply(new CreateScope { Name = "New Scope 3" })).Data!.NewEntityId;

        id1.Should().NotBe(id2!.Value).And.NotBe(id3!.Value);
        id2.Should().NotBe(id3!.Value);
    }

    [Test]
    public void CreateScope_WithNullName_ShouldThrowArgumentException()
    {
        var svc = GetScopesService();
        Func<Task> action = async () => await svc.Apply(new CreateScope { Name = null! });

        action.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public void CreateScope_WithEmptyName_ShouldThrowArgumentException()
    {
        var svc = GetScopesService();

        Func<Task> a1 = async () => await svc.Apply(new CreateScope { Name = "" });
        Func<Task> a2 = async () => await svc.Apply(new CreateScope { Name = " " });
        Func<Task> a3 = async () => await svc.Apply(new CreateScope { Name = "   " });

        a1.Should().ThrowAsync<ArgumentException>();
        a2.Should().ThrowAsync<ArgumentException>();
        a3.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task GetScopes_ShouldReturnEmptyList()
    {
        var svc = GetScopesService();

        var r1 = await svc.GetScopes();
        r1.Succeeded.Should().BeTrue();
        r1.Data.Should().BeEmpty();

        var r2 = await svc.GetScopes();
        r2.Succeeded.Should().BeTrue();
        r2.Data.Should().BeEmpty();
    }

    [Test]
    public async Task GetScopes_All_ShouldReturnListWith3Scopes()
    {
        var svc = GetScopesService();

        await svc.Apply(new CreateScope { Name = "New Scope 1" });
        await svc.Apply(new CreateScope { Name = "New Scope 2" });
        await svc.Apply(new CreateScope { Name = "New Scope 3" });

        var result = await svc.GetScopes();

        result.Succeeded.Should().BeTrue();
        result.Data.Should().HaveCount(3);
    }

    [Test]
    public async Task GetScopes_NotActive_ShouldReturnListWith2Scopes()
    {
        var svc = GetScopesService();

        var id1 = (await svc.Apply(new CreateScope { Name = "New Scope 1" })).Data!.NewEntityId;
        await svc.Apply(new CreateScope { Name = "New Scope 2" });
        var id3 = (await svc.Apply(new CreateScope { Name = "New Scope 3" })).Data!.NewEntityId;
        var id4 = (await svc.Apply(new CreateScope { Name = "New Scope 4" })).Data!.NewEntityId;

        var tid1 = (await svc.Apply(new CreateTheme { ScopeId = id1!.Value, Name = "Theme 1", Actual = false })).Data!.NewEntityId;
        await svc.Apply(new CreateTheme { ScopeId = id3!.Value, Name = "Theme 2", Actual = true });
        var tid2 = (await svc.Apply(new CreateTheme { ScopeId = id3!.Value, Name = "Theme 3", Actual = false })).Data!.NewEntityId;
        await svc.Apply(new CreateTheme { ScopeId = id4!.Value, Name = "Theme 4", Actual = true });

        var result = await svc.GetScopes();
        var list = result.Data!.Where(s => s.Themes.Any(t => !t.Actual)).ToList();

        list.Should().HaveCount(2);
        list.Should().SatisfyRespectively(
            s =>
            {
                s.Id.Should().Be(id1!.Value);
                s.Themes.Should().ContainSingle(t => t.Id == tid1);
            },
            s =>
            {
                s.Id.Should().Be(id3!.Value);
                s.Themes.Should().ContainSingle(t => t.Id == tid2);
            });
    }

    [Test]
    public async Task GetScopes_Active_ShouldReturnListWith2Scopes()
    {
        var svc = GetScopesService();

        var id1 = (await svc.Apply(new CreateScope { Name = "New Scope 1" })).Data!.NewEntityId;
        await svc.Apply(new CreateScope { Name = "New Scope 2" });
        var id3 = (await svc.Apply(new CreateScope { Name = "New Scope 3" })).Data!.NewEntityId;
        var id4 = (await svc.Apply(new CreateScope { Name = "New Scope 4" })).Data!.NewEntityId;

        await svc.Apply(new CreateTheme { ScopeId = id1!.Value, Name = "Theme 1", Actual = false });
        var tid1 = (await svc.Apply(new CreateTheme { ScopeId = id3!.Value, Name = "Theme 2", Actual = true })).Data!.NewEntityId;
        await svc.Apply(new CreateTheme { ScopeId = id3!.Value, Name = "Theme 3", Actual = false });
        var tid2 = (await svc.Apply(new CreateTheme { ScopeId = id4!.Value, Name = "Theme 4", Actual = true })).Data!.NewEntityId;

        var result = await svc.GetScopes();
        var list = result.Data!.Where(s => s.Themes.Any(t => t.Actual)).ToList();

        list.Should().HaveCount(2);
        list.Should().SatisfyRespectively(
            s =>
            {
                s.Id.Should().Be(id3!.Value);
                s.Themes.Should().ContainSingle(t => t.Id == tid1);
            },
            s =>
            {
                s.Id.Should().Be(id4!.Value);
                s.Themes.Should().ContainSingle(t => t.Id == tid2);
            });
    }

    [Test]
    public async Task GetScopes_OnlyActive_ShouldReturnListWith1Scope()
    {
        var svc = GetScopesService();

        var id1 = (await svc.Apply(new CreateScope { Name = "New Scope 1" })).Data!.NewEntityId;
        await svc.Apply(new CreateTheme { ScopeId = id1!.Value, Name = "Not actual theme", Actual = false });

        await svc.Apply(new CreateScope { Name = "New Scope 2" });

        var id2 = (await svc.Apply(new CreateScope { Name = "New Scope 3" })).Data!.NewEntityId;
        await svc.Apply(new CreateTheme { ScopeId = id2!.Value, Name = "Actual theme", Actual = true });

        var result = await svc.GetScopes();
        var list = result.Data!.Where(s => s.Themes.Any(t => t.Actual)).ToList();

        list.Should().HaveCount(1);
        list[0].Id.Should().Be(id2!.Value);
        list[0].ScopeName.Should().Be("New Scope 3");
    }

    [Test]
    public async Task DeleteScope_WithNotExistingId_ShouldNotThrowException()
    {
        var svc = GetScopesService();
        var result = await svc.DeleteScope(Guid.NewGuid());

        result.Succeeded.Should().BeTrue();
    }

    [Test]
    public async Task CreateTheme_ShouldNotThrowException()
    {
        var context = CreateContext();
        var svc = GetScopesService(context);
        var scopeId = CreateScope(context, "Test Scope");

        var result = await svc.Apply(new CreateTheme
        {
            ScopeId = scopeId,
            Name = @"!@#$%^''""&*()_+,.<><>?//[]||\\апрорпывоаъъЇЇііі.єєєйй",
            Actual = false
        });

        result.Succeeded.Should().BeTrue();
        result.Data!.NewEntityId.Should().NotBeEmpty();
    }

    [Test]
    public async Task DeleteTheme_WithNotExistingId_ShouldNotThrowException()
    {
        var svc = GetScopesService();
        var result = await svc.DeleteTheme(Guid.NewGuid());

        result.Succeeded.Should().BeTrue();
    }

    [Test]
    public async Task ThemeActuality_ChangeTwice()
    {
        var context = CreateContext();
        var scopes = Create3ScopesWith1ThemeForEach(context);
        var themes = scopes.SelectMany(s => s.Themes).ToList();
        var svc = GetScopesService(context);

        await svc.Apply(new UpdateTheme { ThemeId = themes[0].Id, Actual = true });
        await svc.Apply(new UpdateTheme { ThemeId = themes[1].Id, Actual = false });
        await svc.Apply(new UpdateTheme { ThemeId = themes[2].Id, Actual = true });

        context.Themes.Single(t => t.Id == themes[0].Id).Actual.Should().BeTrue();
        context.Themes.Single(t => t.Id == themes[1].Id).Actual.Should().BeFalse();
        context.Themes.Single(t => t.Id == themes[2].Id).Actual.Should().BeTrue();

        await svc.Apply(new UpdateTheme { ThemeId = themes[2].Id, Actual = false });

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
        t!.Deleted.Should().BeTrue();

        var rt = context.RecordThemes.IgnoreQueryFilters().FirstOrDefault(recTheme => recTheme.ThemeId == theme.Id);
        rt.Should().NotBeNull();
        rt!.Deleted.Should().BeTrue();
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
        var id = (await svc.Apply(new CreateScope { Name = "New Scope 1" })).Data!.NewEntityId;

        await svc.DeleteScope(id!.Value);

        var scope = await context.Scopes.FindAsync(id);
        scope.Should().NotBeNull();
        scope!.Deleted.Should().BeTrue();
    }

    [Test]
    public async Task DeleteScope_WithDisabledSD_ShouldDeleteRecords()
    {
        var context = CreateContext();
        context.SoftDeleting = false;
        var svc = GetScopesService(context);
        var id = (await svc.Apply(new CreateScope { Name = "New Scope 1" })).Data!.NewEntityId;

        await svc.DeleteScope(id!.Value);

        var scope = await context.Scopes.FindAsync(id);
        scope.Should().BeNull();
    }
}
