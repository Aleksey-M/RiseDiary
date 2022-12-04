namespace RiseDiary.Shared.Images;

public sealed class RotateImageDto
{
    public Guid ImageId { get; set; }

    public bool RotateRight { get; set; }
}

public sealed class ReplaceImageDto
{
    public Guid ImageId { get; set; }

    public string ContentType { get; set; } = string.Empty;
}

public sealed class SaveAsNewImageDto
{
    public Guid ImageId { get; set; }

    public Guid? RecordId { get; set; }
}