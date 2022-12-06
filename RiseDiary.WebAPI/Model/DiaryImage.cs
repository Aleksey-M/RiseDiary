namespace RiseDiary.Model;

public sealed class DiaryImage : IDeletedEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public DateTime CreateDate { get; set; }

    public DateTime ModifyDate { get; set; }

    public byte[] Thumbnail { get; set; } = Array.Empty<byte>();

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

public sealed class DiaryImageFull
{
    public Guid Id { get; set; }

    public Guid ImageId { get; set; }

    public byte[] Data { get; set; } = Array.Empty<byte>();
}

public sealed class TempImage
{
    public Guid Id { get; set; }

    public Guid SourceImageId { get; set; }

    public string Modification { get; set; } = "";

    public byte[] Data { get; set; } = Array.Empty<byte>();

    public int Width { get; set; }

    public int Height { get; set; }

    public long SizeByte { get; set; }

    public string ContentType { get; set; } = string.Empty;
}

public static class ImageExtensions
{
    public static string GetBase64Thumbnail(this DiaryImage image) => Convert.ToBase64String(image.Thumbnail);
}
