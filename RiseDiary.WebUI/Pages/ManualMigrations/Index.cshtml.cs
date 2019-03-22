using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Pages.ManualMigrations
{
    public class IndexModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public IndexModel(DiaryDbContext context)
        {
            _context = context;
        }
        public void OnGet()
        {

        }

        public async Task OnPostUpdateAllCodesAsync()
        {
            (await _context.Scopes.ToListAsync()).ForEach(s => s.Code = Guid.NewGuid().ToString());
            (await _context.Themes.ToListAsync()).ForEach(t => t.Code = Guid.NewGuid().ToString());
            (await _context.Images.ToListAsync()).ForEach(i => i.Code = Guid.NewGuid().ToString());
            (await _context.Records.ToListAsync()).ForEach(r => r.Code = Guid.NewGuid().ToString());
            (await _context.Cogitations.ToListAsync()).ForEach(c => c.Code = Guid.NewGuid().ToString());
            await _context.SaveChangesAsync();
        }
    }
}