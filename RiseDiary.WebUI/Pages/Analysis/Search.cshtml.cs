using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Pages.Analysis
{
    public class IndexModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public string SearchString;
        public List<DiaryRecord> Records;

        public IndexModel(DiaryDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                SearchString = string.Empty;
                Records = new List<DiaryRecord>();
            }
            else
            {
                SearchString = searchString;
                Records = await _context.SearchRecordsByText(SearchString);
            }            
        }               
    }
}