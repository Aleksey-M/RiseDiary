using Microsoft.AspNetCore.Components;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Records
{
    public partial class SetStartRecordComponent
    {
        [Parameter]
        public Guid RecordId { get; set; }
        [Inject]
        public DiaryDbContext DbContext { get; set; } = null!;
        private Guid? StartRecordId { get; set; }
        private string GliphIconName
        {
            get
            {
                if (StartRecordId == null) return "glyphicon-plus";
                if (StartRecordId == RecordId) return "glyphicon-remove";
                return "glyphicon-refresh";
            }
        }

        private async Task SetOnStartPage()
        {
            await DbContext.UpdateAppSetting(AppSettingsKeys.StartPageRecordId, RecordId.ToString()).ConfigureAwait(false);
            await ReadStartPageId().ConfigureAwait(false);
        }

        private async Task RemoveFromStartPage()
        {
            await DbContext.UpdateAppSetting(AppSettingsKeys.StartPageRecordId, string.Empty).ConfigureAwait(false);
            StartRecordId = null;
            await ReadStartPageId().ConfigureAwait(false);
        }

        private async Task ReadStartPageId()
        {
            var s = await DbContext.GetAppSetting(AppSettingsKeys.StartPageRecordId).ConfigureAwait(false) ?? string.Empty;
            if (Guid.TryParse(s, out var id))
            {
                StartRecordId = id;
            }
            await InvokeAsync(() => StateHasChanged()).ConfigureAwait(false);
        }

        protected override async Task OnInitializedAsync()
        {
            await ReadStartPageId().ConfigureAwait(false);
        }
    }
}
