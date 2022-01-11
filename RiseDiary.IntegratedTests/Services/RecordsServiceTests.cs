using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RiseDiary.IntegratedTests.Stubs;
using RiseDiary.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace RiseDiary.IntegratedTests.Services
{
    [TestFixture]
    internal class RecordsServiceTests : TestedServices
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
            var id = Create_Record(context);
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
            var id = Create_Record(context);
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
            var id = Create_Record(context);
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
            var id = Create_Record(context);
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
            var id = Create_Record(context);
            var rec = await context.Records.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id);

            await svc.UpdateRecord(id, null, null, null);

            var updatedRecord = await context.Records.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id);
            updatedRecord.Date.Should().Be(rec.Date);
            updatedRecord.Name.Should().Be(rec.Name);
            updatedRecord.Text.Should().Be(rec.Text);
            updatedRecord.CreateDate.Should().Be(rec.CreateDate);
            updatedRecord.ModifyDate.Should().Be(rec.ModifyDate);
        }

        [Test]
        public async Task AddDiaryRecord_ShouldReplaceHostAndPortByPlaceholder()
        {
            var context = CreateContext();
            var svc = GetRecordsService(context);
            var hpSvc = new HostAndPortStub();
            var text = $@"New text with link {hpSvc.GetHostAndPort()}/index and another link {hpSvc.GetHostAndPort()}/images/12345678";

            var id = await svc.AddRecord(DateOnly.FromDateTime(DateTime.UtcNow), text, text);

            var rec = await context.Records.SingleOrDefaultAsync(r => r.Id == id);
            rec.Name.Should().NotContain(hpSvc.GetHostAndPort());
            rec.Text.Should().NotContain(hpSvc.GetHostAndPort());
            rec.Name.Should().Contain(hpSvc.GetHostAndPortPlaceholder(), Exactly.Twice());
            rec.Text.Should().Contain(hpSvc.GetHostAndPortPlaceholder(), Exactly.Twice());
        }

        [Test]
        public async Task UpdateRecord_ShouldReplaceHostAndPortByPlaceholder()
        {
            var context = CreateContext();
            var svc = GetRecordsService(context);
            var id = Create_Record(context);
            var hpSvc = new HostAndPortStub();
            var text = $@"New text with link {hpSvc.GetHostAndPort()}/index and another link {hpSvc.GetHostAndPort()}/images/12345678";

            await svc.UpdateRecord(id, null, text, text);

            var rec = await context.Records.SingleOrDefaultAsync(r => r.Id == id);
            rec.Name.Should().NotContain(hpSvc.GetHostAndPort());
            rec.Text.Should().NotContain(hpSvc.GetHostAndPort());
            rec.Name.Should().Contain(hpSvc.GetHostAndPortPlaceholder(), Exactly.Twice());
            rec.Text.Should().Contain(hpSvc.GetHostAndPortPlaceholder(), Exactly.Twice());
        }

        [Test]
        public async Task FetchRecordById_ShouldReplacePlaceholderByHostAndPort()
        {
            var context = CreateContext();
            var svc = GetRecordsService(context);
            var rec = GetTestRecord();
            var hpSvc = new HostAndPortStub();
            var text = $@"New text with link {hpSvc.GetHostAndPortPlaceholder()}/index and another link {hpSvc.GetHostAndPortPlaceholder()}/images/12345678";
            rec.Name = text;
            rec.Text = text;
            context.Records.Add(rec);
            await context.SaveChangesAsync();

            var savedRec = await svc.FetchRecordById(rec.Id);

            savedRec.Name.Should().NotContain(hpSvc.GetHostAndPortPlaceholder());
            savedRec.Text.Should().NotContain(hpSvc.GetHostAndPortPlaceholder());
            savedRec.Name.Should().Contain(hpSvc.GetHostAndPort(), Exactly.Twice());
            savedRec.Text.Should().Contain(hpSvc.GetHostAndPort(), Exactly.Twice());
        }

        [Test]
        public async Task FetchRecordById_ShouldReplacePlaceholderByHostAndPort_InCogitations()
        {
            var context = CreateContext();
            var svc = GetRecordsService(context);
            var recId = Create_Record(context);
            var hpSvc = new HostAndPortStub();
            var text = $@"New text with link {hpSvc.GetHostAndPortPlaceholder()}/index and another link {hpSvc.GetHostAndPortPlaceholder()}/images/12345678";
            var cogitationsList = Enumerable.Range(1, 3)
                .Select(i => new Cogitation { Id = Guid.NewGuid(), RecordId = recId, Date = DateTime.UtcNow, Text = text }).ToList();
            context.Cogitations.AddRange(cogitationsList);
            await context.SaveChangesAsync();

            var savedRec = await svc.FetchRecordById(recId);

            foreach (var c in savedRec.Cogitations)
            {
                c.Text.Should().NotContain(hpSvc.GetHostAndPortPlaceholder());
                c.Text.Should().Contain(hpSvc.GetHostAndPort(), Exactly.Twice());
            }
        }


        [Test]
        public async Task AddCogitation_WithNullParameter_ShouldThrowArgumentException()
        {
            var context = CreateContext();
            var svc = GetRecordsService();
            var recId = Create_Record(context);

            Func<Task> action = async () => await svc.AddCogitation(recId, null!);

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Test]
        public async Task AddCogitation_ShouldAddNewCogitation()
        {
            var context = CreateContext();
            var svc = GetRecordsService(context);
            var recId = Create_Record(context);
            var text = "Some text";

            var cogId = await svc.AddCogitation(recId, text);

            var cog = await context.Cogitations.FindAsync(cogId);
            cog.Should().NotBeNull();
            cog.Text.Should().Be(text);
            cog.Date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(500));
            cog.RecordId.Should().Be(recId);
        }

        [Test]
        public async Task AddCogitation_ShouldReplaceHostAndPortByPlaceholdere()
        {
            var context = CreateContext();
            var svc = GetRecordsService(context);
            var recId = Create_Record(context);
            var hpSvc = new HostAndPortStub();
            var text = $@"New text with link {hpSvc.GetHostAndPort()}/index and another link {hpSvc.GetHostAndPort()}/images/12345678";

            var cogId = await svc.AddCogitation(recId, text);

            var cog = await context.Cogitations.FindAsync(cogId);
            cog.Text.Should().NotContain(hpSvc.GetHostAndPort());
            cog.Text.Should().Contain(hpSvc.GetHostAndPortPlaceholder(), Exactly.Twice());
        }

        [Test]
        public async Task UpdateCogitationText_ShouldUpdate()
        {
            var context = CreateContext();
            var svc = GetRecordsService(context);
            var recId = Create_Record(context);
            var cId = Guid.NewGuid();
            var cDate = DateTime.UtcNow;
            var cText = "Some Text 81237912y0r9182ny";
            var newText = "Some other text";
            context.Cogitations.Add(new Cogitation { Id = cId, Date = cDate, RecordId = recId, Text = cText });
            await context.SaveChangesAsync();

            await svc.UpdateCogitationText(cId, newText);

            var updatedCog = await context.Cogitations.FindAsync(cId);
            updatedCog.Text.Should().Be(newText);
            updatedCog.Date.Should().Be(cDate);
        }

        [Test]
        public async Task UpdateCogitationText_ShouldReplaceHostAndPortByPlaceholdere()
        {
            var context = CreateContext();
            var svc = GetRecordsService(context);
            var recId = Create_Record(context);
            var cId = Guid.NewGuid();
            var cDate = DateTime.UtcNow;
            var cText = "Some Text 81237912y0r9182ny";
            var hpSvc = new HostAndPortStub();
            var newText = $@"New text with link {hpSvc.GetHostAndPort()}/index and another link {hpSvc.GetHostAndPort()}/images/12345678";
            context.Cogitations.Add(new Cogitation { Id = cId, Date = cDate, RecordId = recId, Text = cText });
            await context.SaveChangesAsync();

            await svc.UpdateCogitationText(cId, newText);

            var cog = await context.Cogitations.FindAsync(cId);
            cog.Text.Should().NotContain(hpSvc.GetHostAndPort());
            cog.Text.Should().Contain(hpSvc.GetHostAndPortPlaceholder(), Exactly.Twice());
        }

        [Test]
        public async Task SoftDeleting_DeleteCogitation_WithEnabledSD_ShouldMarkAsDeleted()
        {
            var context = CreateContext();
            var svc = GetRecordsService(context);
            context.SoftDeleting = true;
            var (_, cogId) = Create_3Records_1Cogitation(context);

            await svc.DeleteCogitation(cogId);

            var cogitation = await context.Cogitations.FindAsync(cogId);
            cogitation.Should().NotBeNull();
            cogitation.Deleted.Should().BeTrue();
        }

        [Test]
        public async Task SoftDeleting_DeleteCogitation_WithDisabledSD_ShouldDeleteRecords()
        {
            var context = CreateContext();
            var svc = GetRecordsService(context);
            context.SoftDeleting = false;
            var (_, cogId) = Create_3Records_1Cogitation(context);

            await svc.DeleteCogitation(cogId);

            var cogitation = await context.Cogitations.FindAsync(cogId);
            cogitation.Should().BeNull();
        }
    }
}
