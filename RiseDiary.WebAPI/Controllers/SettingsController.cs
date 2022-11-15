using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared;

namespace RiseDiary.Api;

[ApiController]
[Route("api/settings")]
public sealed class SettingsController : ControllerBase
{
    private readonly IAppSettingsService _settingsSvc;

    public SettingsController(IAppSettingsService settingsSvc)
    {
        _settingsSvc = settingsSvc;
    }

    [HttpGet, Route("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AppSettingDto>> GetSettings(AppSettingsKey key)
    {
        var (value, modifiedDate) = await _settingsSvc.GetAppSetting(key);

        return new AppSettingDto
        {
            Key = key,
            Value = value ?? "",
            ModifiedDate = modifiedDate ?? default
        };
    }

    [HttpPut, Route("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSettingValue(AppSettingsKey key, string value)
    {
        await _settingsSvc.UpdateAppSetting(key, value);

        return NoContent();
    }
}
