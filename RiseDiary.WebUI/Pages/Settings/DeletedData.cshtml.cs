using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Settings
{
    public class IndexModel : PageModel
    {
        private readonly ISqliteDatabase _sqliteDatabase;

        public IndexModel(ISqliteDatabase sqliteDatabase)
        {
            _sqliteDatabase = sqliteDatabase;
        }

        public DeletedData DeletedData { get; private set; } = new();

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            try
            {
                DeletedData = await _sqliteDatabase.GetDeletedEntitiesData(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
