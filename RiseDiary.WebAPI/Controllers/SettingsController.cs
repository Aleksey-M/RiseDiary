using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared;
using RiseDiary.Shared.Settings;
using RiseDiary.WebUI.Model;

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

    [HttpGet("{key}")]
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

    [HttpPut("{key}")]
    public async Task<IActionResult> UpdateSettingValue(AppSettingsKey key, AppSettingDto value)
    {
        if (value.Key != key) return BadRequest(new { Message = "Not consistent request" });

        await _settingsSvc.UpdateAppSetting(key, value.Value);

        return NoContent();
    }

    [HttpGet("images")]
    public async Task<ActionResult<ImagesSettings>> GetImagesSettings(CancellationToken cancellationToken) =>
        await _settingsSvc.GetImagesSettings(cancellationToken);


    [HttpPut("images")]
    public async Task<IActionResult> UpdateImagesSettings(
        [FromServices] ImagesSettingsValidator validator, ImagesSettings imagesSettings)
    {
        validator.ValidateAndThrow(imagesSettings);

        await _settingsSvc.UpdateImagesSettings(imagesSettings);

        return NoContent();
    }

    [HttpGet("pages")]
    public async Task<ActionResult<PagesSizesSettings>> GetPagesSettings(CancellationToken cancellationToken) =>
        await _settingsSvc.GetPagesSizesSettings(cancellationToken);


    [HttpPut("pages")]
    public async Task<IActionResult> UpdatePagesSettings(
        [FromServices] PagesSizesSettingsValidator validator, PagesSizesSettings pagesSizesSettings)
    {
        validator.ValidateAndThrow(pagesSizesSettings);

        await _settingsSvc.UpdatePagesSizesSettings(pagesSizesSettings);

        return NoContent();
    }

    [HttpGet("dates")]
    public async Task<ActionResult<ImportantDaysSettings>> GetDatesSettings(CancellationToken cancellationToken) =>
        await _settingsSvc.GetImportantDaysSettings(cancellationToken);


    [HttpPut("dates")]
    public async Task<IActionResult> UpdateDatesSettings(
        [FromServices] ImportantDaysSettingsValidator validator, ImportantDaysSettings importantDaysSettings)
    {
        validator.ValidateAndThrow(importantDaysSettings);

        await _settingsSvc.UpdateImportantDaysSettings(importantDaysSettings);

        return NoContent();
    }
}
