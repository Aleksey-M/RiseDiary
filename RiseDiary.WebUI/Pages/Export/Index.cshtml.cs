using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RiseDiary.Model.ImportExport;
using RiseDiary.WebUI.Data;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Export
{
    public class IndexModel : PageModel
    {
        private DiaryDbContext _context;
        public IndexModel(DiaryDbContext context)
        {
            _context = context;
        }
        public int RecordsCount { get; private set; }
        public int CogitationsCount { get; private set; }
        public int ImagesCount { get; private set; }
        public int ScopesCount { get; private set; }
        public int ThemesCount { get; private set; }
        public void OnGet()
        {
            RecordsCount = _context.Records.Count();
            CogitationsCount = _context.Cogitations.Count();
            ImagesCount = _context.Images.Count();
            ScopesCount = _context.Scopes.Count();
            ThemesCount = _context.Themes.Count();
        }

        [BindProperty]
        public bool Images { get; set; } = true;
        [BindProperty]
        public bool Themes { get; set; } = true;

        public async Task<FileResult> OnPostExportAsync()
        {
            var records = await _context.Records.OrderByDescending(r => r.Date).Select(r => r.Id).ToListAsync();

            var basePath = Request.Scheme + Uri.SchemeDelimiter + Request.Host;

            var str = await _context.SerializeDiaryRecords(records, basePath, Themes, Images);

            var fileType = @"text/xml";
            var fileName = $@"Export_{DateTime.Now.ToString("yyyy.MM.dd_hh.mm.ss")}.xml";

            return File(Encoding.UTF8.GetBytes(str), fileType, fileName);
        }
    }
}