using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using RiseDiary.WebUI.Data;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.DbFile
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
    public class CompressBaseModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly DiaryDbContext _context;

        public CompressBaseModel(IConfiguration config, DiaryDbContext context)
        {
            _config = config;
            _context = context;
        }

        public string FileSize { get; private set; }
        public string CompressedFileSize { get; private set; }

        public void OnGet()
        {
            FileSize = Math.Round(new FileInfo(_config.GetValue<string>("dbFile")).Length / 1024f / 1024f, 2).ToString() + " Mb";
        }

        public async Task OnPostCompressBaseAsync()
        {
            FileSize = Math.Round(new FileInfo(_config.GetValue<string>("dbFile")).Length / 1024f / 1024f, 2).ToString() + " Mb";
            await _context.Vacuum();
            CompressedFileSize = Math.Round(new FileInfo(_config.GetValue<string>("dbFile")).Length / 1024f / 1024f, 2).ToString() + " Mb";
        }
    }
}