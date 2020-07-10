using Microsoft.AspNetCore.Mvc;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Api
{
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly DiaryDbContext _context;
        public SettingsController(DiaryDbContext context)
        {
            _context = context;
        }
    }
}
