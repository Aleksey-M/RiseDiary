using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.Shared;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace RiseDiary.WebUI
{
    public class ImageQualityModel : PageModel
    {
        private readonly IAppSettingsService _settingsSvc;

        public ImageQualityModel(IAppSettingsService settingsSvc)
        {
            _settingsSvc = settingsSvc;
        }

        [Required(ErrorMessage = "Обязательное значение"), Range(1, 100, ErrorMessage = "Допустимы значения от 1 до 100")]
        public int? ImageQuality { get; set; }

        [Required(ErrorMessage = "Обязательное значение"), Range(50, 500, ErrorMessage = "Допустимы значения от 50 до 500")]
        public int? ThumbnailSize { get; private set; }

        [Required(ErrorMessage = "Обязательное значение"), Range(200, 2000, ErrorMessage = "Допустимы значения от 200 до 2000")]
        public int? CropImageMaxScaledWidth { get; set; }

        [Required(ErrorMessage = "Обязательное значение"), Range(200, 2000, ErrorMessage = "Допустимы значения от 200 до 2000")]
        public int? CropImageMaxScaledHeight { get; set; }


        private async Task LoadSettings()
        {
            ImageQuality = await _settingsSvc.GetAppSettingInt(AppSettingsKey.ImageQuality).ConfigureAwait(false);
            ThumbnailSize = await _settingsSvc.GetAppSettingInt(AppSettingsKey.ThumbnailSize).ConfigureAwait(false);
            CropImageMaxScaledHeight = await _settingsSvc.GetAppSettingInt(AppSettingsKey.CropImageMaxScaledHeight).ConfigureAwait(false);
            CropImageMaxScaledWidth = await _settingsSvc.GetAppSettingInt(AppSettingsKey.CropImageMaxScaledWidth).ConfigureAwait(false);
        }

        public async Task OnGetAsync()
        {
            await LoadSettings();
        }

        public async Task OnPostUpdateImageQualityAsync(int imageQuality)
        {
            if (imageQuality > 0 && imageQuality <= 100)
            {
                await _settingsSvc.UpdateAppSetting(AppSettingsKey.ImageQuality, $"{imageQuality}").ConfigureAwait(false);
            }

            await LoadSettings();
        }

        public async Task OnPostUpdateThumbnailSizeAsync(int thumbnailSize)
        {
            if (thumbnailSize > 50 && thumbnailSize <= 500)
            {
                await _settingsSvc.UpdateAppSetting(AppSettingsKey.ThumbnailSize, $"{thumbnailSize}").ConfigureAwait(false);
            }

            await LoadSettings();
        }

        public async Task OnPostUpdateMaxScaledWidthAsync(int cropImageMaxScaledWidth)
        {
            if (cropImageMaxScaledWidth > 200 && cropImageMaxScaledWidth <= 2000)
            {
                await _settingsSvc.UpdateAppSetting(AppSettingsKey.CropImageMaxScaledWidth, $"{cropImageMaxScaledWidth}").ConfigureAwait(false);
            }

            await LoadSettings();
        }

        public async Task OnPostUpdateMaxScaledHeightAsync(int cropImageMaxScaledHeight)
        {
            if (cropImageMaxScaledHeight > 200 && cropImageMaxScaledHeight <= 2000)
            {
                await _settingsSvc.UpdateAppSetting(AppSettingsKey.CropImageMaxScaledHeight, $"{cropImageMaxScaledHeight}").ConfigureAwait(false);
            }

            await LoadSettings();
        }
    }
}