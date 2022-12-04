namespace RiseDiary.Shared.Images;

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

    public int Width { get; set; }

    public int Height { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public DateTime? Taken { get; set; }

    public string CameraModel { get; set; } = "";

    public Dictionary<Guid, string> ImageLinks { get; set; } = new();

    public TempImageDto? TempImage { get; set; }

    public int GetBiggestDimension() => Math.Max(Width, Height);
}
