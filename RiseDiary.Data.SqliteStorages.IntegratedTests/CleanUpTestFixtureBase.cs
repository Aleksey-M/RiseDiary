using NUnit.Framework;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests
{
    public class CleanUpTestFixtureBase
    {
        [OneTimeTearDown]
        public void CleanUp()
        {
            TestsHelper.RemoveTmpDbFiles();
        }
    }
}
