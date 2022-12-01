namespace RiseDiary.Shared.Images;

public sealed class ImagesPageDto
{
    public PagesInfo PagesInfo { get; set; } = new();

    public List<ImageListItemDto> Images { get; set; } = new();
}


public sealed class ImageListItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public int Width { get; set; }

    public int Height { get; set; }

    public string SizeKb { get; set; } = "";

    public string Base64Thumbnail { get; set; } = "";
}