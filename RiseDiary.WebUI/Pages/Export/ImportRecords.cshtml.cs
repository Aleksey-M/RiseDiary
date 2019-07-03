using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Pages.Export
{
    public enum UpdateRecordAction { SkipNew, RewriteOld, RewriteIfNewest }
    public class ImportRecordsModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public ImportRecordsModel(DiaryDbContext context)
        {
            _context = context;
        }
        public void OnGet()
        {

        }

        //public async Task OnPostImportDataAsync(IFormFile xmlFile, UpdateRecordAction updateAction)
        //{
        //    if (xmlFile == null) return;

        //}
    }
}