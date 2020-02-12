using Microsoft.AspNetCore.Components;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
namespace RiseDiary.WebUI.Pages
{
    public partial class ScopesListComponent
    {
        [Inject] public DiaryDbContext DbContext { get; set; } = null!;
        private List<DiaryScope> Scopes { get; set; } = null!;
        private string? NewScopeName { get; set; }
        private string? ErrorMessage { get; set; }

        private async Task CreateNewScope()
        {
            if (string.IsNullOrWhiteSpace(NewScopeName))
            {
                ErrorMessage = "Название области не введено";
            }
            else
            {
                await DbContext.AddScope(NewScopeName.Trim());
                NewScopeName = null;
                Scopes = await DbContext.FetchScopesWithThemes();
                ErrorMessage = null;
            }
        }

        private async Task UpdateState()
        {
            Scopes = await DbContext.FetchScopesWithThemes();
            this.StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            Scopes = await DbContext.FetchScopesWithThemes();
        }
    }
}
