using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using RiseDiary.WebUI.Data;
using System;
using System.Globalization;
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

        public string FileSize { get; private set; } = string.Empty;
        public string CompressedFileSize { get; private set; } = string.Empty;

        public void OnGet()
        {
            FileSize = Math.Round(new FileInfo(_config.GetValue<string>("dbFile")).Length / 1024f / 1024f, 2).ToString(CultureInfo.InvariantCulture) + " Mb";
        }

        public async Task OnPostCompressBaseAsync()
        {
            FileSize = Math.Round(new FileInfo(_config.GetValue<string>("dbFile")).Length / 1024f / 1024f, 2).ToString(CultureInfo.InvariantCulture) + " Mb";
            await _context.Vacuum();
            CompressedFileSize = Math.Round(new FileInfo(_config.GetValue<string>("dbFile")).Length / 1024f / 1024f, 2).ToString(CultureInfo.InvariantCulture) + " Mb";
        }
    }
}