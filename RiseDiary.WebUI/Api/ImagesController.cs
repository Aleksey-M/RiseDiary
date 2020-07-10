using Microsoft.AspNetCore.Mvc;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Api
{
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly DiaryDbContext _context;
        public ImagesController(DiaryDbContext context)
        {
            _context = context;
        }


    }
}
