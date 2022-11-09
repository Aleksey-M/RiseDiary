using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RiseDiary.WebAPI.Shared.Dto;

public sealed class ScaleDownDto
{
    [Required]
    public Guid ImageId { get; set; }

    [Required, Range(10, 10_000)]
    public int NewImageBiggestSide { get; set; }
}

public sealed class RotateImageDto
{
    [Required]
    public Guid ImageId { get; set; }

    [Required]
    public bool RotateRight { get; set; }
}

public sealed class ReplaceImageDto
{
    [Required]
    public Guid ImageId { get; set; }

    [Required]
    public IFormFile? Image { get; set; }
}

public sealed class ScaledImagePreviewDto
{
    public double Coefficient { get; set; }

    public string ImageBase64String { get; set; } = "";
}

public sealed class CropImageDto
{
    [Required]
    public Guid ImageId { get; set; }

    [Required, Range(1.0, 100.0)]
    public double Coefficient { get; set; }

    [Required, Range(0, 100_000)]
    public int Left { get; set; }

    [Required, Range(0, 100_000)]
    public int Top { get; set; }

    [Required, Range(0, 100_000)]
    public int Width { get; set; }

    [Required, Range(0, 100_000)]
    public int Height { get; set; }
}
