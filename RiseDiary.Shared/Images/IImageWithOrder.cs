namespace RiseDiary.Shared.Images;

public interface IImageWithOrder
{
    public Guid ImageId { get; set; }

    public int Order { get; set; }
}
