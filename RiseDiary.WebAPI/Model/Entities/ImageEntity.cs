namespace RiseDiary.Model;


/// <summary>
/// Информация о изображении
/// </summary>
public sealed class ImageEntity : IDeletableEntity
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
    public byte[] Image { get; set; } = [];
    public TempImageEntity? TempImage { get; set; }
    public ICollection<RecordImageEntity> RecordsRefs { get; private set; } = null!;
}
