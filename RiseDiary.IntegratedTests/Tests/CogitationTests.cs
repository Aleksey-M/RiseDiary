using NUnit.Framework;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.IntegratedTests
{
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning disable CA1812
#pragma warning disable CA1303
    [TestFixture]
    internal class CogitationTests : TestFixtureBase
    {
        [Test]
        public void AddCogitation_ShouldThrowDbUpdateException()
        {
            var context = CreateContext();
            var cogitation = new Cogitation { Date = DateTime.Now, RecordId = Guid.NewGuid(), Text = @"qrhpqfuшвгарщшйрпйшга  йщрайзца" };

            Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateException>(async () => _ = await context.AddCogitation(cogitation, "host"));

            //Assert.LessOrEqual(0, id);
        }

        [Test]
        public void AddCogitation_WithNullParameter_ShouldThrowArgumentNullException()
        {
            var context = CreateContext();

            Assert.ThrowsAsync<ArgumentNullException>(async () => _ = await context.AddCogitation(null!, ""));
        }

        [Test]
        public async Task DeleteCogitation_ShouldDeleteCogitation()
        {
            var context = CreateContext();
            var (recId, cogId) = Create_3Records_1Cogitation(context);

            await context.DeleteCogitation(cogId);

            Assert.Null(context.Cogitations.SingleOrDefault(c => c.Id == cogId));
        }

        [Test]
        public Task DeleteCogitation_WithNullParameter_ShouldNotThrowException()
        {
            var context = CreateContext();
            return context.DeleteCogitation(Guid.NewGuid());
        }

        [Test]
        public async Task FetchCogitationById_ShouldReturnCogitation()
        {
            var context = CreateContext();
            var (recId, cogId) = Create_3Records_1Cogitation(context);

            var cog = await context.FetchCogitationById(cogId, "");

            Assert.IsNotNull(cog);
            Assert.AreEqual(recId, cog?.RecordId);
        }

        [Test]
        public async Task FetchCogitationById_WithNotExistingId_ShouldReturnNull()
        {
            var context = CreateContext();

            var cog = await context.FetchCogitationById(Guid.NewGuid(), "host");

            Assert.IsNull(cog);
        }

        [Test]
        public async Task FetchAllCogitationsOfRecord_ShouldReturn2Cogitation()
        {
            var context = CreateContext();
            var recIds = Create_3Records_3_2_1Cogitations(context);

            var resList = await context.FetchAllCogitationsOfRecord(recIds[1], "");

            Assert.IsNotNull(resList);
            Assert.AreEqual(2, resList.Count);
        }

        [Test]
        public async Task FetchAllCogitationsOfRecord_WithNotExistingRecId_ShouldReturnEmptyList()
        {
            var context = CreateContext();

            var resList = await context.FetchAllCogitationsOfRecord(Guid.NewGuid(), "host");

            Assert.IsNotNull(resList);
            Assert.Zero(resList.Count);
        }

        [Test]
        public async Task UpdateCogitation_ShouldUpdateDateAndText()
        {
            var context = CreateContext();
            var (_, cogId) = Create_3Records_1Cogitation(context);
            var cogitation = await context.Cogitations.FindAsync(cogId);
            var cogOldDate = cogitation.Date;
            var cogOldText = cogitation.Text;

            cogitation.Date = DateTime.Now.AddDays(-23);
            cogitation.Text = ";ijgvirnvirg;rgivuw;rgw";
            await context.UpdateCogitation(cogitation, "host");

            cogitation = await context.Cogitations.FindAsync(cogId);

            Assert.AreNotEqual(cogOldDate, cogitation.Date);
            Assert.AreNotEqual(cogOldText, cogitation.Text);
        }

        [Test]
        public void UpdateCogitation_WithNotExistingId_ShouldThrowArgumentException()
        {
            var context = CreateContext();
            Assert.ThrowsAsync<ArgumentException>(async () => await context.UpdateCogitation(new Cogitation { Id = Guid.NewGuid() }, ""));
        }

        [Test]
        public async Task GetCogitationsCount_ShouldReturn3()
        {
            var context = CreateContext();
            var recIds = Create_3Records_3_2_1Cogitations(context);

            var count = await context.GetCogitationsCount(recIds[0]);

            Assert.AreEqual(3, count);
        }

        [Test]
        public async Task GetCogitationsCount_WithNotExistingRecId_ShouldReturn0()
        {
            var context = CreateContext();

            var count = await context.GetCogitationsCount(Guid.NewGuid());

            Assert.Zero(count);
        }
    }
}
