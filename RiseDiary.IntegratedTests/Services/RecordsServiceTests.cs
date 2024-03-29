﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace RiseDiary.IntegratedTests.Services;

[TestFixture]
internal class RecordsServiceTests : TestFixtureBase
{
    [Test]
    public async Task AddRecord_ShouldCreateRecord()
    {
        var context = CreateContext();
        var svc = GetRecordsService(context);
        var recDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3));
        var recName = "Record Name";
        var recText = @"<H1>Record text</H1>";

        var id = await svc.AddRecord(recDate, recName, recText);

        var rec = await context.Records.SingleOrDefaultAsync(r => r.Id == id);
        rec.Should().NotBeNull();
        rec.Name.Should().Be(recName);
        rec.Text.Should().Be(recText);
        rec.Date.Should().Be(recDate);
        rec.CreateDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(500));
        rec.ModifyDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(500));
    }

    [Test]
    public async Task FetchRecord_ShouldReturnSavedRecord()
    {
        var context = CreateContext();
        var svc = GetRecordsService(context);
        var id = CreateRecord(context);
        var rec = await context.Records.SingleOrDefaultAsync(r => r.Id == id);

        var loadedRec = await svc.FetchRecordById(id);

        loadedRec.Should().NotBeNull();
        loadedRec.Date.Should().Be(rec.Date);
        loadedRec.CreateDate.Should().Be(rec.CreateDate);
        loadedRec.ModifyDate.Should().Be(rec.ModifyDate);
        loadedRec.Name.Should().Be(rec.Name);
        loadedRec.Text.Should().Be(rec.Text);

        loadedRec.Cogitations.Should().NotBeNull();
        loadedRec.Cogitations.Should().BeEmpty();
        loadedRec.ThemesRefs.Should().NotBeNull();
        loadedRec.ThemesRefs.Should().BeEmpty();
        loadedRec.ImagesRefs.Should().NotBeNull();
        loadedRec.ImagesRefs.Should().BeEmpty();
    }

    [Test]
    public async Task SoftDeleting_DeleteRecord_WithEnabledSD_ShouldMarkAsDeleted()
    {
        var context = CreateContext();
        context.SoftDeleting = true;
        (var rec, _, _) = CreateEntities(context);
        var svc = GetRecordsService(context);

        await svc.DeleteRecord(rec.Id);

        var deletedRecord = await context.Records.FindAsync(rec.Id);
        deletedRecord.Should().NotBeNull();
        deletedRecord.Deleted.Should().BeTrue();

        var deletedCogitation = await context.Cogitations.IgnoreQueryFilters().FirstOrDefaultAsync(cog => cog.RecordId == rec.Id);
        deletedCogitation.Should().NotBeNull();
        deletedCogitation.Deleted.Should().BeTrue();

        var recordTheme = await context.RecordThemes.IgnoreQueryFilters().FirstOrDefaultAsync(recTheme => recTheme.RecordId == rec.Id);
        recordTheme.Should().NotBeNull();
        recordTheme.Deleted.Should().BeTrue();

        var recordImage = await context.RecordImages.IgnoreQueryFilters().FirstOrDefaultAsync(recImage => recImage.RecordId == rec.Id);
        recordImage.Should().NotBeNull();
        recordImage.Deleted.Should().BeTrue();
    }

    [Test]
    public async Task SoftDeleting_DeleteRecord_WithDisabledSD_ShouldDeleteRecords()
    {
        var context = CreateContext();
        context.SoftDeleting = false;
        (var rec, _, _) = CreateEntities(context);
        var svc = GetRecordsService(context);

        await svc.DeleteRecord(rec.Id);

        var deletedRecord = await context.Records.FindAsync(rec.Id);
        deletedRecord.Should().BeNull();

        var deletedCogitation = await context.Cogitations.IgnoreQueryFilters().FirstOrDefaultAsync(cog => cog.RecordId == rec.Id);
        deletedCogitation.Should().BeNull();

        var recordTheme = await context.RecordThemes.IgnoreQueryFilters().FirstOrDefaultAsync(recTheme => recTheme.RecordId == rec.Id);
        recordTheme.Should().BeNull();

        var recordImage = await context.RecordImages.IgnoreQueryFilters().FirstOrDefaultAsync(recImage => recImage.RecordId == rec.Id);
        recordImage.Should().BeNull();
    }

    [Test]
    public async Task UpdateRecord_Date_ShouldUpdateDate()
    {
        var context = CreateContext();
        var svc = GetRecordsService(context);
        var id = CreateRecord(context);
        var rec = await context.Records.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id);
        var newDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));

        await svc.UpdateRecord(id, newDate, null, null);

        var updatedRecord = await context.Records.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id);
        updatedRecord.Date.Should().Be(newDate);
        updatedRecord.Name.Should().Be(rec.Name);
        updatedRecord.Text.Should().Be(rec.Text);
        updatedRecord.CreateDate.Should().Be(rec.CreateDate);
        updatedRecord.ModifyDate.Should().BeAfter(rec.ModifyDate);
    }

    [Test]
    public async Task UpdateRecord_Name_ShouldUpdateName()
    {
        var context = CreateContext();
        var svc = GetRecordsService(context);
        var id = CreateRecord(context);
        var rec = await context.Records.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id);
        var newName = "Some new name 123456789";

        await svc.UpdateRecord(id, null, newName, null);

        var updatedRecord = await context.Records.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id);
        updatedRecord.Date.Should().Be(rec.Date);
        updatedRecord.Name.Should().Be(newName);
        updatedRecord.Text.Should().Be(rec.Text);
        updatedRecord.CreateDate.Should().Be(rec.CreateDate);
        updatedRecord.ModifyDate.Should().BeAfter(rec.ModifyDate);
    }

    [Test]
    public async Task UpdateRecord_Text_ShouldUpdateText()
    {
        var context = CreateContext();
        var svc = GetRecordsService(context);
        var id = CreateRecord(context);
        var rec = await context.Records.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id);
        var newText = @"Some new text <p>dfefbsbsdfbsdfbdfb</p>";

        await svc.UpdateRecord(id, null, null, newText);

        var updatedRecord = await context.Records.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id);
        updatedRecord.Date.Should().Be(rec.Date);
        updatedRecord.Name.Should().Be(rec.Name);
        updatedRecord.Text.Should().Be(newText);
        updatedRecord.CreateDate.Should().Be(rec.CreateDate);
        updatedRecord.ModifyDate.Should().BeAfter(rec.ModifyDate);
    }

    [Test]
    public async Task UpdateRecord_NoneNew_ShouldNotUpdateAnyField()
    {
        var context = CreateContext();
        var svc = GetRecordsService(context);
        var id = CreateRecord(context);
        var rec = await context.Records.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id);

        await svc.UpdateRecord(id, null, null, null);

        var updatedRecord = await context.Records.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id);
        updatedRecord.Date.Should().Be(rec.Date);
        updatedRecord.Name.Should().Be(rec.Name);
        updatedRecord.Text.Should().Be(rec.Text);
        updatedRecord.CreateDate.Should().Be(rec.CreateDate);
        updatedRecord.ModifyDate.Should().Be(rec.ModifyDate);
    }
}
