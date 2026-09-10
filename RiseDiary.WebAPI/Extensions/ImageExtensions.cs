using RiseDiary.Model;

namespace RiseDiary.WebAPI.Extensions;

public static class ImageExtensions
{
    /// <summary>
    /// Получение строкового представления превью картинки для встраивания в html
    /// </summary>
    public static string GetBase64Thumbnail(this ImageEntity image) => Convert.ToBase64String(image.Thumbnail);
}
