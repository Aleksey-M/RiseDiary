using RiseDiary.Common.Images;
using RiseDiary.Model;

/// <summary>
/// Сущность для привязки изображений к записи (связ много ко многим)
/// </summary>
public sealed class RecordImageEntity : IDeletableEntity, IImageWithOrder
{
    public Guid ImageId { get; set; }
    public Guid RecordId { get; set; }
    public bool Deleted { get; set; }
    public int Order { get; set; }
    public ImageEntity? Image { get; set; }
    public RecordEntity? Record { get; set; }
}
