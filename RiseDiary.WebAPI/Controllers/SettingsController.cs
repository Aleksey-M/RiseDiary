using Microsoft.AspNetCore.Mvc;
using RiseDiary.Common.Core;
using RiseDiary.Common.Settings;
using RiseDiary.Common.Settings.Validators;
using RiseDiary.WebAPI.Settings;
using RiseDiary.WebAPI.Settings.Model;
using RiseDiary.WebAPI.Settings.Services;
using System.Text.Json;

namespace RiseDiary.WebAPI.Controllers;

[ApiController]
[Route("api/settings")]
public sealed class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsSvc;
    private readonly ISettingsDtoValidator _dtoValidator;
    private readonly JsonSerializerOptions _jsonOptions;

    public SettingsController(ISettingsService settingsSvc, ISettingsDtoValidator dtoValidator, JsonSerializerOptions jsonOptions)
    {
        _settingsSvc = settingsSvc;
        _dtoValidator = dtoValidator;
        _jsonOptions = jsonOptions;
    }

    private readonly string[] _settingsKeys = ["bookmarks", "pages", "images", "dates"];
    private static readonly Dictionary<string, (Type model, Type dto)> _settingTypes = new()
    {
        ["bookmarks"] = (model: typeof(BookmarksSettings), dto: typeof(BookmarksSettingsDto)),
        ["pages"] = (model: typeof(PagesSizesSettings), dto: typeof(PagesSizesSettingsDto)),
        ["images"] = (model: typeof(ImagesSettings), dto: typeof(ImagesSettingsDto)),
        ["dates"] = (model: typeof(ImportantDaysSettings), dto: typeof(ImportantDaysSettingsDto))
    };

    [HttpGet("{key}")]
    public async Task<IActionResult> GetSettings(string key, CancellationToken cancellationToken)
    {
        key = key.ToLower();
        if (!_settingsKeys.Contains(key))
        {
            return NotFound();
        }

        object? result = (key) switch
        {
            "bookmarks" => (await _settingsSvc.GetSetting<BookmarksSettings>(cancellationToken)).Data,
            "pages" => (await _settingsSvc.GetSetting<PagesSizesSettings>(cancellationToken)).Data,
            "images" => (await _settingsSvc.GetSetting<ImagesSettings>(cancellationToken)).Data,
            "dates" => (await _settingsSvc.GetSetting<ImportantDaysSettings>(cancellationToken)).Data,
            _ => null
        };

        return result == null
            ? NotFound()
            : Ok(result);
    }

    [HttpPut("{key}")]
    public async Task<IActionResult> UpdateSettingValue(string key, [FromBody] JsonElement payload)
    {
        if (!_settingTypes.TryGetValue(key.ToLower(), out var type))
        {
            return BadRequest();
        }

        var payloadText = payload.GetRawText();
        var typedDto = JsonSerializer.Deserialize(payloadText, type.dto, _jsonOptions);

        var validateResult = _dtoValidator.ValidateDto(typedDto);
        if (!validateResult.Succeeded)
        {
            return BadRequest(validateResult);
        }

        var method = typeof(SettingsService).GetMethod(nameof(_settingsSvc.UpdateSetting));

        var generic = method?.MakeGenericMethod(type.model);
        if (generic == null)
        {
            return BadRequest();
        }

        var typedModel = JsonSerializer.Deserialize(payloadText, type.model, _jsonOptions);
        var result = generic.Invoke(_settingsSvc, [typedModel]);
        if (result is Task<Result> task)
        {
            await task;
            return Ok(task.Result);
        }

        return NoContent();
    }
}
