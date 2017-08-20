using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests
{
    [TestFixture]
    public class DataBaseTests : CleanUpTestFixtureBase
    {        
        [Test]
        public async Task GetConnection_ShouldCreateDbFile()
        {
            var dbManager = new DataBaseManager(TestsHelper.DirNameFull);            
            string fileNameFull = Path.Combine(TestsHelper.DirNameFull, dbManager.DbFileName);
            if (File.Exists(fileNameFull))
            {
                File.Delete(fileNameFull);
            }

            var conn = await dbManager.GetConnection();
            
            Assert.IsNotNull(conn);
            Assert.IsTrue(File.Exists(fileNameFull));
            Assert.Pass(fileNameFull);
        }
    }
}
