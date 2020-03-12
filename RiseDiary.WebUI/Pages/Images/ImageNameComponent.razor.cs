using Microsoft.AspNetCore.Components;
using RiseDiary.WebUI.Data;
using System;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Images
{
    public partial class ImageNameComponent
    {
        [Inject]
        public DiaryDbContext DbContext { get; set; } = null!;
        [Parameter]
        public string ImageName { get; set; } = null!;
        [Parameter]
        public Guid ImageId { get; set; }
        public bool EditMode { get; set; }
        public string? ErrorMessage { get; set; }

        public void ToEditMode()
        {
            EditMode = true;
        }

        public async Task SaveImageName()
        {
            var imageName = ImageName.Trim();
            
            if(imageName.Length == 0)
            {
                ErrorMessage = "Новое название не введено!";
                return;
            }

            if(imageName.Length > 250)
            {
                ErrorMessage = "Длина названия не должна превышать 250 символов";
                return;
            }

            await DbContext.UpdateImageName(ImageId, imageName).ConfigureAwait(false);
            ImageName = imageName;
            ErrorMessage = null;
            EditMode = false;
        }
    }
}
