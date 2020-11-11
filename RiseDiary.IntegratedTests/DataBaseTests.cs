using FluentAssertions;
using NUnit.Framework;
using System.IO;

namespace RiseDiary.IntegratedTests
{
#pragma warning disable CA1812
    [TestFixture]
    internal class SQLite_DataBaseTests : TestFixtureBase
    {
        [Test]
        public void GetConnection_ShouldCreateDbFile_ByDbContext()
        {
            var (context, fileName) = GetContextWithFileName();

            context.Should().NotBeNull();
            File.Exists(fileName).Should().BeTrue();
        }
    }
}
