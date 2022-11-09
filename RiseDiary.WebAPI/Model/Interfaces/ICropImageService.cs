using System.Drawing;

namespace RiseDiary.Model;

public interface ICropImageService
{
    Task<ScaledImagePreview> CreateScaledImagePreview(Guid imageId);

    Task CropImage(Guid imageId, Rectangle selectedPreviewRect, double scaleCoefficient);
}
