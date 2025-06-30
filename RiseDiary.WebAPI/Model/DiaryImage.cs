namespace RiseDiary.Model;


/// <summary>
/// Информация о изображении
/// </summary>
public sealed class DiaryImage : IDeletedEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public DateTime CreateDate { get; set; }

    public DateTime ModifyDate { get; set; }

    public byte[] Thumbnail { get; set; } = [];

    public int Width { get; set; }

    public int Height { get; set; }

    public long SizeByte { get; set; }

    public bool Deleted { get; set; }

    public DateTime? Taken { get; set; }

    public string? CameraModel { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public DiaryImageFull? FullImage { get; set; }

    public TempImage? TempImage { get; set; }

    public ICollection<DiaryRecordImage> RecordsRefs { get; private set; } = null!;
}


/// <summary>
/// Запись для хранения изображдения в бинарном виде в базе
/// </summary>
public sealed class DiaryImageFull
{
    public Guid Id { get; set; }

    public Guid ImageId { get; set; }

    public byte[] Data { get; set; } = [];
}


/// <summary>
/// Сущность для хранения информации о изображении в процессе его редактирования
/// </summary>
public sealed class TempImage
{
    public Guid Id { get; set; }

    public Guid SourceImageId { get; set; }

    public string Modification { get; set; } = "";

    public byte[] Data { get; set; } = [];

    public int Width { get; set; }

    public int Height { get; set; }

    public long SizeByte { get; set; }

    public string ContentType { get; set; } = string.Empty;
}



public static class ImageExtensions
{
    /// <summary>
    /// Получение строкового представления превью картинки для встраивания в html
    /// </summary>
    public static string GetBase64Thumbnail(this DiaryImage image) => Convert.ToBase64String(image.Thumbnail);
}
