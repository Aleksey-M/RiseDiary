using System;
using System.Drawing;
using System.Threading.Tasks;

namespace RiseDiary.Model
{
    public interface ICropImageService
    {
        Task<ScaledImagePreview> CreateScaledImagePreview(Guid imageId);
        Task CropImage(Guid imageId, Rectangle selectedPreviewRect, double scaleCoefficient);
    }
}
