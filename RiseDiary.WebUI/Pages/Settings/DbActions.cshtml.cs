using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;

namespace RiseDiary.WebUI.Pages.Settings
{
    public class DbActionsModel : PageModel
    {
        private readonly ISqliteDatabase _sqliteDb;

        public DeletedEntitiesCount DeletedEntities { get; private set; } = null!;

        public SqliteDatabaseFileInfo DatabaseFileInfo { get; private set; } = null!;

        public DbActionsModel(ISqliteDatabase sqliteDb)
        {
            _sqliteDb = sqliteDb;
        }


        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            try
            {
                DeletedEntities = await _sqliteDb.GetDeletedEntitiesCount(cancellationToken);
                DatabaseFileInfo = _sqliteDb.GetSqliteDatabaseInfo();
            }
            catch (OperationCanceledException)
            {
            }
        }

        public async Task OnPostClearBaseAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _sqliteDb.ClearDatabase();
                await _sqliteDb.Vacuum();

                DeletedEntities = await _sqliteDb.GetDeletedEntitiesCount(cancellationToken);
                DatabaseFileInfo = _sqliteDb.GetSqliteDatabaseInfo();
            }
            catch (OperationCanceledException)
            {
            }
        }

        public async Task OnPostFile2FileMigrationAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _sqliteDb.File2FileMigration();

                DeletedEntities = await _sqliteDb.GetDeletedEntitiesCount(cancellationToken);
                DatabaseFileInfo = _sqliteDb.GetSqliteDatabaseInfo();
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
