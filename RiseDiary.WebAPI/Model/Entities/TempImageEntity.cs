namespace RiseDiary.Model;

/// <summary>
/// Сущность для хранения информации о изображении в процессе его редактирования
/// </summary>
public sealed class TempImageEntity
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
