using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Threading.Tasks;

#pragma warning disable CA1822 // Mark members as static
namespace RiseDiary.WebUI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DiaryDbContext _context;        
        public IndexModel(DiaryDbContext context)
        {
            _context = context;
        }

        public DiaryRecord? StartPageRecord { get; private set; }
        private string LocalHostAndPort => Request.Scheme + @"://" + Request.Host.Host + ":" + Request.Host.Port;

        public async Task OnGetAsync()
        {
            var sId = await _context.GetAppSetting(AppSettingsKeys.StartPageRecordId).ConfigureAwait(false);
            if(Guid.TryParse(sId, out var id))
            {
                StartPageRecord = await _context.FetchRecordById(id, LocalHostAndPort).ConfigureAwait(false);
            }
        }
    }
}
