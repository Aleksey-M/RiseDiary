using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.Shared;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Settings
{
    public class PageSizeModel : PageModel
    {
        private readonly IAppSettingsService _appSettingsService;

        public PageSizeModel(IAppSettingsService appSettingsService)
        {
            _appSettingsService = appSettingsService;
        }

        [Required(ErrorMessage = "Обязательное значение"), Range(1, 200, ErrorMessage = "Допустимы значения от 1 до 200")]
        public int? ImagesPageSize { get; set; }

        [Required(ErrorMessage = "Обязательное значение"), Range(1, 200, ErrorMessage = "Допустимы значения от 1 до 200")]
        public int? RecordsPageSize { get; private set; }

        [Required(ErrorMessage = "Обязательное значение"), Range(1, 200, ErrorMessage = "Допустимы значения от 1 до 200")]
        public int? AvailableImagesPageSize { get; private set; }

        private async Task LoadSettings()
        {
            ImagesPageSize = await _appSettingsService.GetAppSettingInt(AppSettingsKey.ImagesPageSize).ConfigureAwait(false);
            RecordsPageSize = await _appSettingsService.GetAppSettingInt(AppSettingsKey.RecordsPageSize).ConfigureAwait(false);
            AvailableImagesPageSize = await _appSettingsService.GetAppSettingInt(AppSettingsKey.AvailableImagesPageSize).ConfigureAwait(false);
        }

        public async Task OnGetAsync()
        {
            await LoadSettings(); 
        }

        public async Task OnPostUpdateImagesPageSizeAsync(int imagesPageSize)
        {
            if (imagesPageSize >= 1 && imagesPageSize <= 200)
            {
                await _appSettingsService.UpdateAppSetting(AppSettingsKey.ImagesPageSize, 
                    $"{imagesPageSize}").ConfigureAwait(false);
            }

            await LoadSettings();
        }

        public async Task OnPostUpdateAvailableImagesPageSizeAsync(int availableImagesPageSize)
        {
            if (availableImagesPageSize >= 1 && availableImagesPageSize <= 200)
            {
                await _appSettingsService.UpdateAppSetting(AppSettingsKey.AvailableImagesPageSize, 
                    $"{availableImagesPageSize}").ConfigureAwait(false);
            }

            await LoadSettings();
        }

        public async Task OnPostUpdateRecordsPageSizeAsync(int recordsPageSize)
        {
            if (recordsPageSize >= 1 && recordsPageSize <= 200)
            {
                await _appSettingsService.UpdateAppSetting(AppSettingsKey.RecordsPageSize, 
                    $"{recordsPageSize}").ConfigureAwait(false);
            }

            await LoadSettings();
        }
    }
}
