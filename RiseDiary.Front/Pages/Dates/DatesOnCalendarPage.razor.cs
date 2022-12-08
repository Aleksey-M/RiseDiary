using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RiseDiary.Shared.Calendar;

namespace RiseDiary.Front.Pages.Dates;

public partial class DatesOnCalendarPage : UIComponentBase
{

    [Inject]
    public NavigationManager NavManager { get; set; } = null!;

    [Inject]
    public HttpClient Http { get; set; } = null!;

    [Inject]
    public ILogger<DatesOnCalendarPage> Logger { get; set; } = null!;

    [Inject]
    public IJSRuntime JsRuntime { get; set; } = null!;

    private List<CalendarDateDto>? _datesList;

    private DotNetObjectReference<DateClickHandler>? _handlerRef;

    private List<CalendarDateDto>? _selectedDates;
    private string? _selectedDatesHeader;

    private Lazy<Task<IJSObjectReference>> _moduleTask = null!;
    private Lazy<Task<IJSObjectReference>> _calendarLibTask = null!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _calendarLibTask = new(() => JsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./lib/calendar/dist/js-year-calendar.min.js").AsTask());

        _moduleTask = new(() => JsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./Pages/Dates/DatesOnCalendarPage.razor.js").AsTask());

        try
        {
            await StartApiRequest();

            _datesList = await Http.GetFromJsonAsync<List<CalendarDateDto>>("api/dates/calendar", Token);

            var datesOnly = (_datesList ?? new())
                .Select(x => x.StartDate)
                .Distinct()
                .ToList();

            _handlerRef = await CreateCalendar(datesOnly, OnDateClick);

            await FinishApiRequest(null);
        }
        catch (Exception exc)
        {
            _datesList = new();
            Logger.LogWarning(exc, "Error on initializing");
            await FinishApiRequest(exc.Message);
        }
    }

    private void OnDateClick(DateOnly date)
    {
        _selectedDates = null;
        _selectedDatesHeader = null;

        if (_datesList != null && _datesList.Any(x => x.StartDate == date))
        {
            _selectedDates = _datesList.Where(x => x.StartDate == date).ToList();
            _selectedDatesHeader = $"События {date:yyyy.MM.dd}";
        }

        StateHasChanged();
    }

    private void ClearSelected()
    {
        _selectedDates = null;
        _selectedDatesHeader = null;
    }

    public string GetRecordLink(CalendarDateDto dto) => NavManager.GetUriWithQueryParameters("records/view",
        new Dictionary<string, object?>
        {
            ["recordId"] = dto.Id
        });

    protected override async ValueTask DisposeComponent()
    {
        _handlerRef?.Dispose();
        _handlerRef = null;

        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }

        if (_calendarLibTask.IsValueCreated)
        {
            var module = await _calendarLibTask.Value;
            await module.DisposeAsync();
        }
    }

    public async Task<DotNetObjectReference<DateClickHandler>> CreateCalendar(List<DateOnly> dates, Action<DateOnly> onDateClick)
    {
        _ = await _calendarLibTask.Value;
        var module = await _moduleTask.Value;        

        var dateClickHandler = new DateClickHandler(onDateClick);
        var dateClickHandlerRef = dateClickHandler.CreateRefForJs();

        await module.InvokeVoidAsync("initCalendar", "#calendar", dates, dateClickHandlerRef);

        return dateClickHandlerRef;
    }
}
