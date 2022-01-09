using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using System;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Records
{
    public class CreateModel : PageModel
    {
        private readonly IRecordsService _recordsService;

        public CreateModel(IRecordsService recordsService)
        {
            _recordsService = recordsService;
        }

        public async Task<IActionResult> OnPostAsync(DateTime recordDate, string? recordName, string recordText)
        {
            recordText ??= @"<p></p>";
            if (recordText.Length == 0) recordText = @"<p></p>";
            recordName = recordName?.Trim() ?? "";

            Guid recordId = await _recordsService.AddRecord(DateOnly.FromDateTime(recordDate), recordName, recordText);

            return RedirectToPage("View", new { recordId });
        }
    }
}
