using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace RiseDiary.WebUI
{
    public class SelectImagesModel : PageModel
    {
        public Guid RecordId { get; private set; }
        public void OnGet(Guid recordId)
        {
            RecordId = recordId;
        }
    }
}