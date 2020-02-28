using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Images
{
    public partial class SelectImagesComponent
    {
        [Inject]
        public DiaryDbContext DbContext { get; set; } = null!;
        [Inject]
        public IJSRuntime JSRuntime { get; set; } = null!;
        [Parameter]
        public Guid RecordId { get; set; }
        private List<DiaryImage> Images { get; set; } = new List<DiaryImage>();
        private int AllImagesCount { get; set; }
        private HashSet<Guid> RecordImages { get; set; } = null!;

        private async Task LoadListPart()
        {
            int nextCount = 15;
            if (Images.Count + 15 > AllImagesCount)
            {
                nextCount = AllImagesCount - Images.Count;
            }

            var newImages = await DbContext.FetchImageSet(Images.Count, nextCount).ConfigureAwait(false);
            Images.AddRange(newImages);
        }

        private async Task Save()
        {
            var addedImages = new HashSet<Guid>(await DbContext.RecordImages
                .AsNoTracking()
                .Where(ri => ri.RecordId == RecordId)
                .Select(ri => ri.ImageId)
                .ToListAsync().ConfigureAwait(false));

            var deleted = addedImages.Except(RecordImages);
            foreach(var id in deleted)
            {
                await DbContext.RemoveRecordImage(RecordId, id).ConfigureAwait(false);
            }

            var added = RecordImages.Except(addedImages);
            foreach (var id in added)
            {
                await DbContext.AddRecordImage(RecordId, id).ConfigureAwait(false);
            }

            await JSRuntime.InvokeVoidAsync("showSaveMessage");
        }

        private bool CheckedAttribute(Guid imgId) => RecordImages.Any(id => id == imgId);

        private bool HiddenImagesExists() => AllImagesCount != Images.Count;

        private void ToggleSelection(Guid id)
        {
            if (RecordImages.Contains(id))
            {
                RecordImages.Remove(id);
            }
            else
            {
                RecordImages.Add(id);
            }
        }
        protected override async Task OnInitializedAsync()
        {
            RecordImages = new HashSet<Guid>(
                await DbContext.RecordImages
                .AsNoTracking()
                .Where(ri => ri.RecordId == RecordId)
                .Select(ri => ri.ImageId)
                .ToListAsync().ConfigureAwait(false));

            AllImagesCount = await DbContext.GetImagesCount().ConfigureAwait(false);
            await LoadListPart().ConfigureAwait(false);
        }
    }
}
