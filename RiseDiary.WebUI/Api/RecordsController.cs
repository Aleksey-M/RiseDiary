using Microsoft.AspNetCore.Mvc;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Api
{
    [ApiController]
    public class RecordsController : ControllerBase
    {
        private readonly DiaryDbContext _context;
        public RecordsController(DiaryDbContext context)
        {
            _context = context;
        }
    }
}
