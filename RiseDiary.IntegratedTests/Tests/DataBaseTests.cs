using NUnit.Framework;
using System.IO;

namespace RiseDiary.SqliteStorages.IntegratedTests
{
    [TestFixture]
    internal class SQLite_DataBaseTests : TestFixtureBase
    {       
        [Test]
        public void GetConnection_ShouldCreateDbFile_ByDbContext()
        {
            if (UsePostgreSql) Assert.Ignore("This test for SQLite only");

            var (context, fileName) = GetContextWithFileName();

            Assert.IsNotNull(context);
            Assert.IsTrue(File.Exists(fileName));            
        }
    }
}
