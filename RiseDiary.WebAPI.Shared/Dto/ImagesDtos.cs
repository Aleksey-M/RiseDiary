using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RiseDiary.WebAPI.Shared.Dto;

public sealed class UploadImageDto
{
    [StringLength(250)]
    public string ImageName { get; set; } = "";

    [Required]
    public IFormFile? Image { get; set; }

    public Guid? TargetRecordId { get; set; }

    public int? NewBiggestDimension { get; set; }
}

public sealed class UpdateImageDto
{
    [Required]
    public Guid ImageId { get; set; }

    [Required, StringLength(250)]
    public string ImageNewName { get; set; } = "";
}

public sealed class TempImageDto
{
    public Guid Id { get; set; }

    public string Modification { get; set; } = "";

    public string SizeKb { get; set; } = "";

    public int Width { get; set; }

    public int Height { get; set; }
}

public sealed class ImageDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public DateTime CreateDate { get; set; }

    public DateTime ModifyDate { get; set; }

    public string SizeKb { get; set; } = "";

    public string Base64Image { get; set; } = "";

    public int Width { get; set; }

    public int Height { get; set; }

    public DateTime? Taken { get; set; }

    public string CameraModel { get; set; } = "";

    public TempImageDto? TempImage { get; set; }
}

public sealed class ImageListItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public int Width { get; set; }

    public int Height { get; set; }

    public string SizeKb { get; set; } = "";

    public string Base64Thumbnail { get; set; } = "";
}

public sealed class ImagesPageDto
{
    public PagesInfo PagesInfo { get; set; } = null!;

    public List<ImageListItemDto> Images { get; set; } = null!;
}
