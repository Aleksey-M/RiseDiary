using NUnit.Framework;
using System.IO;

namespace RiseDiary.IntegratedTests
{
    [TestFixture]
    internal class SQLite_DataBaseTests : TestFixtureBase
    {
        [Test]
        public void GetConnection_ShouldCreateDbFile_ByDbContext()
        {
            var (context, fileName) = GetContextWithFileName();

            Assert.IsNotNull(context);
            Assert.IsTrue(File.Exists(fileName));
        }
    }
}
