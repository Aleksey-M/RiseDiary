using RiseDiary.Shared.Images;

namespace RiseDiary.Front.Pages.Images;

public sealed class ImageSelection
{
    public int SelLeft { get; set; }

    public int SelTop { get; set; }

    public int SelWidth { get; set; }

    public int SelHeight { get; set; }

    public int ImageNaturalHeight { get; set; }

    public int ImageNaturalWidth { get; set; }

    public int ImageHeight { get; set; }

    public int ImageWidth { get; set; }

    public CropImageDto CreateDto(Guid imageId)
    {
        var biggestDimensionSize = (double)Math.Max(ImageNaturalHeight, ImageNaturalWidth);
        var biggestViewDimensionSize = (double)Math.Max(ImageHeight, ImageWidth);
        var coef = biggestDimensionSize / biggestViewDimensionSize;

        var dto = new CropImageDto
        {
            ImageId = imageId,
            SourceImageHeight = ImageNaturalHeight,
            SourceImageWidth = ImageNaturalWidth,
            Left = (int)Math.Floor(SelLeft * coef),
            Top = (int)Math.Floor(SelTop * coef),
            Width = (int)Math.Floor(SelWidth * coef),
            Height = (int)Math.Floor(SelHeight * coef)
        };

        if (dto.Top + dto.Height >= dto.SourceImageHeight)
        {
            dto.Height = dto.SourceImageHeight - dto.Top;
        }

        if (dto.Left + dto.Width >= dto.SourceImageWidth)
        {
            dto.Width = dto.SourceImageWidth - dto.Left;
        }

        return dto;
    }
}
