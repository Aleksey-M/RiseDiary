using Microsoft.Extensions.Logging;
using RiseDiary.Common.Core;
using RiseDiary.Common.Settings.Validators;
using System.Text.Json;

namespace RiseDiary.Common.Settings.Client;

public sealed class SettingsClient : ISettingsClient
{
    private readonly HttpClient _http;
    private readonly ILogger<SettingsClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ISettingsDtoValidator _validator;

    private static readonly Dictionary<SettingKey, Type> _settingTypes = new()
    {
        [SettingKey.Bookmarks] = typeof(BookmarksSettingsDto),
        [SettingKey.Pages] = typeof(PagesSizesSettingsDto),
        [SettingKey.Images] = typeof(ImagesSettingsDto),
        [SettingKey.Dates] = typeof(ImportantDaysSettingsDto)
    };

    public SettingsClient(
        HttpClient http,
        ILogger<SettingsClient> logger,
        JsonSerializerOptions jsonOptions,
        ISettingsDtoValidator validator)
    {
        _http = http;
        _logger = logger;
        _jsonOptions = jsonOptions;
        _validator = validator;
    }

    public async Task<Result<object?>> GetAsync(SettingKey key, CancellationToken ct = default)
    {
        if (!_settingTypes.TryGetValue(key, out var type))
        {
            return Result<object?>.Failure($"Неизвестная настройка: {key}");
        }

        var response = await _http.GetAsync($"api/settings/{key}", ct);
        if (!response.IsSuccessStatusCode)
        {
            return Result<object?>.Failure($"Ошибка {response.StatusCode}: {response.ReasonPhrase}");
        }

        var json = await response.Content.ReadAsStringAsync(ct);

        return Result<object?>.Success(JsonSerializer.Deserialize(json, type, _jsonOptions));
    }

    public async Task<Result> UpdateAsync(SettingKey key, object value, CancellationToken ct = default)
    {
        if (!_settingTypes.ContainsKey(key))
        {
            return Result.Failure($"Неизвестная настройка: {key}");
        }           

        var validation = _validator.ValidateDto(value);
        if (!validation.Succeeded)
        {
            return Result.Failure(validation.Message!);
        }

        var json = JsonSerializer.Serialize(value, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await _http.PutAsync($"api/settings/{key}", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Ошибка при выполнении запроса к api: {statusCode} {errorMessage}", response.StatusCode, response.ReasonPhrase);
            return Result.Failure(response.ReasonPhrase ?? string.Empty);
        }

        var resultJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<Result>(resultJson);
        if (result == null)
        {
            _logger.LogWarning("Неуспешная десериализация ответа: {resultJson}", resultJson);
            return Result.Failure(resultJson);
        }

        return result;
    }
}
