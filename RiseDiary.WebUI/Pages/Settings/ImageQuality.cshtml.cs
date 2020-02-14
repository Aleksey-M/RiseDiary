using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI
{
    public class ImageQualityModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public ImageQualityModel(DiaryDbContext context)
        {
            _context = context;
        }

        public int ImageQuality { get; private set; }

        public async Task OnGetAsync()
        {
            ImageQuality = await _context.GetAppSettingInt(AppSettingsKeys.ImageQuality).ConfigureAwait(false) ?? 75;
        }

        public async Task OnPostUpdateImageQualityAsync(int imageQuality)
        {
            if(imageQuality > 0 && imageQuality <= 100)
            {
                await _context.UpdateAppSetting(AppSettingsKeys.ImageQuality, $"{imageQuality}").ConfigureAwait(false);
                ImageQuality = imageQuality;
            }
            else
            {
                ImageQuality = await _context.GetAppSettingInt(AppSettingsKeys.ImageQuality).ConfigureAwait(false) ?? 75;
            }
        }
    }
}