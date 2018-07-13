using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RiseDiary.WebUI.Data;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.DbFile
{
    public class ClearBaseModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public ClearBaseModel(DiaryDbContext context)
        {
            _context = context;
        }
        public int DeletedRecordsCount { get; private set; }
        public int DeletedScopesCount { get; private set; }
        public int DeletedThemesCount { get; private set; }
        public int DeletedRecordThemesCount { get; private set; }
        public int DeletedCogitationsCount { get; private set; }
        public int DeletedImagesCount { get; private set; }
        public int DeletedRecordImagesCount { get; private set; }

        public async Task UpdateModel()
        {
            DeletedCogitationsCount = await _context.Cogitations.CountAsync(a => a.Deleted);
            DeletedImagesCount = await _context.Images.CountAsync(a => a.Deleted);
            DeletedRecordImagesCount = await _context.RecordImages.CountAsync(a => a.Deleted);
            DeletedRecordsCount = await _context.Records.CountAsync(a => a.Deleted);
            DeletedRecordThemesCount = await _context.RecordThemes.CountAsync(a => a.Deleted);
            DeletedScopesCount = await _context.Scopes.CountAsync(a => a.Deleted);
            DeletedThemesCount = await _context.Themes.CountAsync(a => a.Deleted);
        }

        public async Task OnGetAsync()
        {
            await UpdateModel();
        }

        public async Task OnPostClearBaseAsync()
        {
            await _context.ClearDbFromDeletedRecords();
            await UpdateModel();
        }
    }
}