using RiseDiary.Common.Core;
using System.Text.Json;

namespace RiseDiary.WebUI.Clients;

public class ScopesClientBase
{
    private readonly JsonSerializerOptions _jsonOptions;

    public ScopesClientBase(JsonSerializerOptions jsonOptions)
    {
        _jsonOptions = jsonOptions;
    }

    protected async Task<Result> ReadResult(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure($"Ошибка выполнения запроса к серверу: {response.StatusCode} - {response.ReasonPhrase}");
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Result>(json, _jsonOptions) ?? Result.Failure($"Ошибка десериализации: {json}");
    }

    protected async Task<Result<T>> ReadResult<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            return Result<T>.Failure($"Ошибка выполнения запроса к серверу: {response.StatusCode} - {response.ReasonPhrase}");
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Result<T>>(json, _jsonOptions) ?? Result<T>.Failure($"Ошибка десериализации: {json}");
    }
}