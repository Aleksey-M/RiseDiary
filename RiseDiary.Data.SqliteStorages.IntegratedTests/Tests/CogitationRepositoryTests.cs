using NUnit.Framework;
using RiseDiary.Data.SqliteStorages.IntegratedTests;
using RiseDiary.Data.SqliteStorages.IntegratedTests.DbContextAdapter;
using RiseDiary.Data.SqliteStorages.IntegratedTests.TestDomain;
using System;
using System.Threading.Tasks;

namespace RiseDiary.SqliteStorages.IntegratedTests
{
    [TestFixture]
    public class CogitationRepositoryTests : CleanUpTestFixtureBase
    {
        [Test]
        public async Task AddCogitation_ShouldNotThrowException()
        {
            var cogRepo = new CogitationRepository(TestHelper.GetClearBase());
            var cogitation = new Cogitation { CogitationDate = DateTime.Now, RecordId = 101, Text = @"qrhpqfuшвгарщшйрпйшга  йщрайзца" };

            int id = await cogRepo.AddCogitation(cogitation);

            Assert.LessOrEqual(0, id);
        }
    }
}
