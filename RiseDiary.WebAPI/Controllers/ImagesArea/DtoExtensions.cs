using RiseDiary.Model;
using RiseDiary.Shared.Images;

namespace RiseDiary.WebAPI.Controllers.ImagesArea;

internal static class DtoExtensions
{
    public static ImageListItemDto ToListDto(this DiaryImage? image) => image == null
        ? throw new ArgumentNullException(nameof(image))
        : new()
        {
            ImageId = image.Id,
            Name = image.Name,
            Width = image.Width,
            Height = image.Height,
            SizeKb = image.SizeByte.ToFileSizeString(),
            Base64Thumbnail = image.GetBase64Thumbnail()
        };

    public static ImageListItemDto ToListDto(this DiaryRecordImage? image) => image == null
        ? throw new ArgumentNullException(nameof(image))
        : new()
        {
            ImageId = image.Image!.Id,
            Name = image.Image.Name,
            Width = image.Image.Width,
            Height = image.Image.Height,
            SizeKb = image.Image.SizeByte.ToFileSizeString(),
            Base64Thumbnail = image.Image.GetBase64Thumbnail(),
            Order = image.Order
        };

    public static TempImageDto? ToDto(this TempImage? tempImage) => tempImage == null ? null : new()
    {
        Id = tempImage.Id,
        Modification = tempImage.Modification,
        Width = tempImage.Width,
        Height = tempImage.Height,
        SizeKb = tempImage.SizeByte.ToFileSizeString()
    };

    public static ImageDto ToDto(this DiaryImage image, Dictionary<Guid, string> imageLinks) => new()
    {
        Id = image.Id,
        Name = image.Name,
        CreateDate = image.CreateDate,
        ModifyDate = image.ModifyDate,
        Width = image.Width,
        Height = image.Height,
        CameraModel = image.CameraModel ?? "",
        Taken = image.Taken,
        SizeKb = image.SizeByte.ToFileSizeString(),
        ContentType = image.ContentType,
        ImageLinks = imageLinks,
        TempImage = image.TempImage.ToDto()
    };
}
