using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using RiseDiary.WebUI;
using RiseDiary.WebUI.Data;
using System.Threading.Tasks;

namespace Migrations
{
    [TestFixture]
    public class OneTimeMigrations
    {
        [Test, Ignore("Special test")]
        public void File2FileMigration()
        {
            var sourceFile = @"D:\Projects\RiseDiary\DB\MigrateDatabaseToNewId\RiseDiary.db";
            SqliteFileBackup.File2FileMigration(sourceFile);
        }

        [Test, Ignore("Special test")]
        public async Task UpdateImageLinksInText()
        {
            var sqliteFile = @"C:\Users\Oleksii.Mamikonian\source\repos\Db\DbRiseDiary.db";
            var connStr = $@"Data Source={sqliteFile};";
            IConfigurationRoot configuration = new ConfigurationBuilder().Build();
            var builder = new DbContextOptionsBuilder<DiaryDbContext>();
            builder.UseSqlite(connStr);
            using var context = new DiaryDbContext(builder.Options);
            context.Database.Migrate();

            var allRecords = context.Records.Include(r => r.Cogitations).AsAsyncEnumerable();
            await foreach (var rec in allRecords)
            {
                var newText = rec.Text.Replace(@"/Images/ImageFile/", @"/api/v1.0/image-file/", true, null);
                rec.Text = newText;

                foreach (var cog in rec.Cogitations)
                {
                    var newCogText = cog.Text.Replace(@"/Images/ImageFile/", @"/api/v1.0/image-file/", true, null);
                    cog.Text = newCogText;
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
