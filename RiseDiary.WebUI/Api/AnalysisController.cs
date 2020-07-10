using Microsoft.AspNetCore.Mvc;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Api
{
    [ApiController]
    public class AnalysisController : ControllerBase
    {
        private readonly DiaryDbContext _context;
        public AnalysisController(DiaryDbContext context)
        {
            _context = context;
        }
    }
}
