using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Settings
{
    public class DbActionsModel : PageModel
    {
        private readonly ISqliteDatabase _sqliteDb;

        public DeletedEntitiesInfo DeletedEntities { get; private set; } = null!;
        public SqliteDatabaseInfo DatabaseInfo { get; private set; } = null!;

        public DbActionsModel(ISqliteDatabase sqliteDb)
        {
            _sqliteDb = sqliteDb;
        }

        public async Task OnGetAsync()
        {
            DeletedEntities = await _sqliteDb.GetDeletedEntitiesInfo();
            DatabaseInfo = _sqliteDb.GetSqliteDatabaseInfo();
        }

        public async Task OnPostClearBaseAsync()
        {
            await _sqliteDb.ClearDatabase();
            await _sqliteDb.Vacuum();

            DeletedEntities = await _sqliteDb.GetDeletedEntitiesInfo();
            DatabaseInfo = _sqliteDb.GetSqliteDatabaseInfo();
        }

        public async Task OnPostFile2FileMigrationAsync()
        {
            await _sqliteDb.File2FileMigration();

            DeletedEntities = await _sqliteDb.GetDeletedEntitiesInfo();
            DatabaseInfo = _sqliteDb.GetSqliteDatabaseInfo();
        }
    }
}
