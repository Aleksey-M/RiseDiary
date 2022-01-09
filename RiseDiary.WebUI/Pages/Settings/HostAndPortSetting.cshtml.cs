using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using RiseDiary.Model;
using RiseDiary.WebUI.Model;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Settings
{
    public class HostAndPortSetingModel : PageModel
    {
        private readonly IAppSettingsService _settingsSvc;

        public HostAndPortSetingModel(IAppSettingsService settingsSvc)
        {
            _settingsSvc = settingsSvc;
        }

        [Required(ErrorMessage = "Обязательное значение"), MinLength(10, ErrorMessage = "Базовый URI не может быть короче 10 символов")]
        public string? HostAndPort { get; set; }

        public string CurrentHostAndPort { get; set; } = "";


        public async Task OnGetAsync()
        {
            HostAndPort = await _settingsSvc.GetHostAndPort();
            CurrentHostAndPort = Request.GetAppBaseUrl();
        }

        public async Task OnPostUpdateHostAndPortAsync(string hostAndPort)
        {
            if (hostAndPort is { Length: < 10 } || hostAndPort is null)
            {
                HostAndPort = await _settingsSvc.GetHostAndPort();
                CurrentHostAndPort = Request.GetAppBaseUrl();
            }
            else
            {
                await _settingsSvc.UpdateAppSetting(RiseDiary.Shared.AppSettingsKey.HostAndPort, hostAndPort ?? "localhost:5000");
                HostAndPort = await _settingsSvc.GetHostAndPort();
                CurrentHostAndPort = Request.GetAppBaseUrl();
            }
        }
    }
}
