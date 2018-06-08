using NUnit.Framework;
using System.Threading.Tasks;

namespace RiseDiary.SqliteStorages.IntegratedTests
{
    [TestFixture]
    internal class AppSettingsTests : TestFixtureBase
    {
        [Test]
        public async Task UpdateSettings_WithCorrectKeyValue_ShouldNotThrowException()
        {

        }

        [Test]
        public async Task UpdateSettings_WithNullKeyValue_ShouldThrowNullArgumentException()
        {

        }

        [Test]
        public async Task UpdateSettings_WithCorrectKeyValue_ShouldWriteModifiedDate()
        {

        }

        [Test]
        public async Task UpdateSettings_ForExistingKey_ShouldUpdateModifiedDate()
        {

        }

        [Test]
        public async Task UpdateSettings_ForExistingKey_ShouldUpdateValue()
        {

        }

        [Test]
        public async Task GetSettings_ForExistingKey_ShouldReturnValue()
        {

        }

        [Test]
        public async Task GetSettings_ForNotExistingKey_ShouldThrowArgumentException()
        {

        }
    }
}
