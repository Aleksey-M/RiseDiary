using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace RiseDiary.WebUI.Pages.Records
{
    public class RecordViewModel : PageModel
    {
        public Guid RecordId { get; private set; }

        public IActionResult OnGet(Guid? recordId)
        {
            if (recordId == null || recordId == Guid.Empty) return Redirect("/records");

            RecordId = recordId.Value;
            return Page();
        }
    }
}