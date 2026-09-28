using Microsoft.AspNetCore.Components;
using RiseDiary.Common.Abstractions.Clients;
using RiseDiary.Common.Core;
using RiseDiary.Common.Records;
using RiseDiary.Common.Search;
using System.Text.Json;

namespace RiseDiary.WebUI.Clients;

public sealed class RecordsSearchClient : ClientBase, IRecordsSearchClient
{
    private readonly HttpClient _http;
    private readonly NavigationManager _navManager;

    public RecordsSearchClient(
        HttpClient http,
        NavigationManager navManager,
        JsonSerializerOptions jsonOptions)
        : base(jsonOptions)
    {
        _http = http;
        _navManager = navManager;
    }

    public async Task<Result<List<RecordDto>>> GetBookmarks(CancellationToken ct = default)
    {
        var response = await _http.GetAsync("api/bookmarks", ct);
        var result = await ReadResult<List<RecordDto>>(response);

        if (result.Succeeded && result?.Data != null)
        {
            result.Data.ForEach(x => x.SetBaseUri(_navManager.BaseUri));
        }

        return result!;
    }

    // ------------------------------------------------------------
    // GET api/records — поиск по фильтрам
    // ------------------------------------------------------------
    public async Task<Result<RecordsPageDto<RecordListItemDto>>> GetRecords(
        RecordsSearchFilters filter,
        int page,
        CancellationToken ct = default)
    {
        var query = BuildQuery(filter, page, expanded: false);

        var response = await _http.GetAsync($"api/records{query}", ct);
        var result = await ReadResult<RecordsPageDto<RecordListItemDto>>(response);

        if (result.Succeeded && result?.Data?.Records != null)
        {
            result.Data.Records.ForEach(x => x.SetBaseUri(_navManager.BaseUri));
        }

        return result!;
    }

    // ------------------------------------------------------------
    // GET api/records?expanded=true — расширенный DTO
    // ------------------------------------------------------------
    public async Task<Result<RecordsPageDto<RecordDto>>> GetRecordsExpanded(
        RecordsSearchFilters filter,
        int page,
        CancellationToken ct = default)
    {
        var query = BuildQuery(filter, page, expanded: true);

        var response = await _http.GetAsync($"api/records{query}", ct);
        var result = await ReadResult<RecordsPageDto<RecordDto>>(response);

        if (result.Succeeded && result?.Data?.Records != null)
        {
            result.Data.Records.ForEach(x => x.SetBaseUri(_navManager.BaseUri));
        }

        return result!;
    }

    // ------------------------------------------------------------
    // GET api/records/search — поиск по тексту
    // ------------------------------------------------------------
    public async Task<Result<RecordsPageDto<RecordListItemDto>>> SearchRecords(
        RecordsTextFilter filter,
        CancellationToken ct = default)
    {
        var query = BuildTextQuery(filter);

        var response = await _http.GetAsync($"api/records/search{query}", ct);
        var result = await ReadResult<RecordsPageDto<RecordListItemDto>>(response);

        if (result.Succeeded && result?.Data?.Records != null)
        {
            result.Data.Records.ForEach(x => x.SetBaseUri(_navManager.BaseUri));
        }

        return result!;
    }

    // ------------------------------------------------------------
    // GET api/records/this-day/{date}
    // ------------------------------------------------------------
    public async Task<Result<RecordListItemDto[]>> GetThisDayRecords(
        DateOnly? date,
        CancellationToken ct = default)
    {
        var dateStr = date?.ToString("yyyy-MM-dd") ?? "";
        var response = await _http.GetAsync($"api/records/this-day/{dateStr}", ct);

        var result = await ReadResult<RecordListItemDto[]>(response);

        if (result.Succeeded && result?.Data != null)
        {
            foreach (var x in result.Data)
            {
                x.SetBaseUri(_navManager.BaseUri);
            }
        }

        return result!;
    }

    // ------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------
    private static string BuildQuery(RecordsSearchFilters filter, int page, bool expanded)
    {
        var q = new List<string>();

        if (filter.From != null)
            q.Add($"from={filter.From:yyyy-MM-dd}");

        if (filter.To != null)
            q.Add($"to={filter.To:yyyy-MM-dd}");

        if (!string.IsNullOrWhiteSpace(filter.Name))
            q.Add($"name={Uri.EscapeDataString(filter.Name)}");

        if (filter.CombinedThemes)
            q.Add("combinedThemes=true");

        if (!string.IsNullOrEmpty(filter.Themes))
            q.Add($"themes={filter.Themes}");

        q.Add($"page={page + 1}");

        if (expanded)
            q.Add("expanded=true");

        return q.Count == 0 ? "" : "?" + string.Join("&", q);
    }

    private static string BuildTextQuery(RecordsTextFilter filter)
    {
        var q = new List<string>();

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
            q.Add($"searchText={Uri.EscapeDataString(filter.SearchText)}");

        q.Add($"page={filter.PageNo}");

        return q.Count == 0 ? "" : "?" + string.Join("&", q);
    }
}
