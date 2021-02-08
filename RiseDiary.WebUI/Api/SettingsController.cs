using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared;
using System;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Api
{
    [ApiController]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly IAppSettingsService _settingsSvc;
        public SettingsController(IAppSettingsService settingsSvc)
        {
            _settingsSvc = settingsSvc;
        }

        [HttpGet, Route("api/v1.0/settings/{key}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AppSettingDto>> GetSettings(AppSettingsKey key)
        {
            try
            {
                var (value, modifiedDate) = await _settingsSvc.GetAppSetting(key);
                return new AppSettingDto
                {
                    Key = key,
                    Value = value ?? "",
                    ModifiedDate = modifiedDate ?? default
                };
            }
            catch (Exception exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpPut, Route("api/v1.0/settings/{key}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateSettingValue(AppSettingsKey key, string value)
        {
            try
            {
                await _settingsSvc.UpdateAppSetting(key, value);
            }
            catch (Exception exc)
            {
                return BadRequest(exc.Message);
            }

            return NoContent();
        }
    }
}
