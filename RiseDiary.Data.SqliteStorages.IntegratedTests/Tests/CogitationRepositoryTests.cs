using NUnit.Framework;
using RiseDiary.Domain.Model;
using System;
using System.Threading.Tasks;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests.Tests
{
    [TestFixture]
    public class CogitationRepositoryTests : CleanUpTestFixtureBase
    {
        [Test]
        public async Task AddCogitation_ShouldNotThrowException()
        {
            var cogRepo = new CogitationRepository(TestsHelper.GetClearBase());
            var cogitation = new Cogitation { CogitationDate = DateTime.Now, RecordId = 101, Text = @"qrhpqfuшвгарщшйрпйшга  йщрайзца" };

            int id = await cogRepo.AddCogitation(cogitation);

            Assert.LessOrEqual(0, id);
        }
    }
}
