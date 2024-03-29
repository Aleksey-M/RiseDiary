﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RiseDiary.Model;

namespace RiseDiary.IntegratedTests.Services;

[TestFixture]
internal class CogitationsServiceTests : TestFixtureBase
{
    [Test]
    public async Task GetCogitations_ShouldReturnCogitations()
    {
        var context = CreateContext();
        var svc = GetCogitationsService(context);
        var recId = CreateRecord(context);
        await Add3CogitationsForEachRecord(context);

        var cogitations = await svc.GetRecordCogitations(recId);

        cogitations.Should().NotBeNull();
        cogitations.Should().HaveCount(3);
    }

    [Test]
    public async Task AddCogitation_WithNullParameter_ShouldThrowArgumentException()
    {
        var context = CreateContext();
        var svc = GetCogitationsService();
        var recId = CreateRecord(context);

        Func<Task> action = async () => await svc.AddCogitation(recId, null!);

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task AddCogitation_ShouldAddNewCogitation()
    {
        var context = CreateContext();
        var svc = GetCogitationsService(context);
        var recId = CreateRecord(context);
        var text = "Some text";

        var cogId = await svc.AddCogitation(recId, text);

        var cog = await context.Cogitations.FindAsync(cogId);
        cog.Should().NotBeNull();
        cog?.Text.Should().Be(text);
        cog?.Date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(500));
        cog?.RecordId.Should().Be(recId);
    }

    [Test]
    public async Task UpdateCogitationText_ShouldUpdate()
    {
        var context = CreateContext();
        var svc = GetCogitationsService(context);
        var recId = CreateRecord(context);
        var cId = Guid.NewGuid();
        var cDate = DateTime.UtcNow;
        var cText = "Some Text 81237912y0r9182ny";
        var newText = "Some other text";
        context.Cogitations.Add(new Cogitation { Id = cId, Date = cDate, RecordId = recId, Text = cText });
        await context.SaveChangesAsync();

        await svc.UpdateCogitationText(cId, newText);

        var updatedCog = await context.Cogitations.FindAsync(cId);
        updatedCog?.Text.Should().Be(newText);
        updatedCog?.Date.Should().Be(cDate);
    }

    [Test]
    public async Task SoftDeleting_DeleteCogitation_WithEnabledSD_ShouldMarkAsDeleted()
    {
        var context = CreateContext();
        var svc = GetCogitationsService(context);
        context.SoftDeleting = true;
        var (_, cogId) = Create3RecordsAnd1Cogitation(context);

        await svc.DeleteCogitation(cogId);

        var cogitation = await context.Cogitations.FindAsync(cogId);
        cogitation.Should().NotBeNull();
        cogitation?.Deleted.Should().BeTrue();
    }

    [Test]
    public async Task SoftDeleting_DeleteCogitation_WithDisabledSD_ShouldDeleteRecords()
    {
        var context = CreateContext();
        var svc = GetCogitationsService(context);
        context.SoftDeleting = false;
        var (_, cogId) = Create3RecordsAnd1Cogitation(context);

        await svc.DeleteCogitation(cogId);

        var cogitation = await context.Cogitations.FindAsync(cogId);
        cogitation.Should().BeNull();
    }
}
