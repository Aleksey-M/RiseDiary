using System.Net.Http.Json;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RiseDiary.Front.AppServices;
using RiseDiary.Shared.Records;

namespace RiseDiary.Front.Pages.RecordsView;

public partial class ViewRecordPage : UIComponentBase
{
    [Inject]
    public MarkdownService MdService { get; set; } = null!;

    [Inject]
    public ILogger<ViewRecordPage> Logger { get; set; } = null!;

    [Inject]
    public CreateRecordValidator CreateValidator { get; set; } = null!;

    [Inject]
    public UpdateRecordValidator UpdateValidator { get; set; } = null!;

    [Inject]
    public HttpClient Http { get; set; } = null!;

    [Inject]
    public NavigationManager NavManager { get; set; } = null!;

    [Inject]
    public IJSRuntime JsRuntime { get; set; } = null!;

    [Parameter]
    public Guid RecordId { get; set; }

    private RecordEditDto? _recordDto;

    private string CreatedDate => _recordDto?.CreatedDate.ToLocalTime().ToString("yyyy.MM.dd HH.mm.ss") ?? string.Empty;
    private string ModifiedDate => _recordDto?.ModifiedDate.ToLocalTime().ToString("yyyy.MM.dd HH.mm.ss") ?? string.Empty;


    private async Task LoadPageData()
    {
        try
        {
            await StartApiRequest();

            _recordDto = await Http.GetFromJsonAsync<RecordEditDto>($"api/records/{RecordId}", Token);
            if (_recordDto == null)
            {
                Logger.LogWarning("Record with Id = '{recordId}' does not exists", RecordId);
                NavManager.NavigateTo("records");
            }

            await FinishApiRequest(null);
        }
        catch (Exception exc)
        {
            Logger.LogWarning("Error on record loading: {errorMessage}", exc.Message);
            await FinishApiRequest(exc.Message);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadPageData();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (RecordId != default && _recordDto != null && _recordDto.RecordId != RecordId)
        {
            await LoadPageData();
        }

        await base.OnParametersSetAsync();
    }

    private async Task DeleteRecord()
    {
        try
        {
            bool conf = await JsRuntime.InvokeAsync<bool>("confirm", "Удалить запись?");

            if (conf)
            {
                await StartApiRequest();
                await Http.DeleteAsync($"api/records/{RecordId}");

                NavManager.NavigateTo("records");
            }
        }

        catch (Exception exc)
        {
            Logger.LogWarning("Error on deleteng record with id = '{recordId}': {errorMessage}", RecordId, exc.Message);
            await FinishApiRequest(exc.Message);
        }
    }

    private async Task UpdateRecord(DateOnly? date, string? name, string? text)
    {
        var dto = new UpdateRecordDto
        {
            Id = RecordId,
            Date = date,
            Name = name,
            Text = text
        };

        UpdateValidator.ValidateAndThrow(dto);

        await Http.PatchAsJsonAsync($"api/records/{RecordId}", dto);

        _recordDto!.Date = dto.Date ?? _recordDto.Date;
        _recordDto!.Name = dto.Name ?? _recordDto.Name;
        _recordDto!.Text = dto.Text ?? _recordDto.Text;
        _recordDto.ModifiedDate = DateTime.Now;

        StateHasChanged();
    }

    private Task UpdateRecordHeader(DateOnly date, string? name) => UpdateRecord(date, name, text: null);
    private Task UpdateRecordText(string text) => UpdateRecord(date: null, name: null, text);
}
