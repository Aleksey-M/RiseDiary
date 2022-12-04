using System.Text.Json.Serialization;
using FluentValidation;

namespace RiseDiary.Shared.Settings;

public sealed class ImagesSettings
{
    public int ImageQuality { get; set; }

    public int ThumbnailSize { get; set; }

    public static IEnumerable<string> SettingsKeys
    {
        get
        {
            yield return nameof(AppSettingsKey.ImageQuality);
            yield return nameof(AppSettingsKey.ThumbnailSize);
        }
    }

    [JsonIgnore]
    public IEnumerable<string> PropertiesValues
    {
        get
        {
            yield return ImageQuality.ToString();
            yield return ThumbnailSize.ToString();
        }
    }
}

public sealed class ImagesSettingsValidator : AbstractValidator<ImagesSettings>
{
    public ImagesSettingsValidator()
    {
        RuleFor(x => x.ImageQuality)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(1, 100).WithMessage("Значение сжатия Jpeg должно быть в пределах от 1 до 100");

        RuleFor(x => x.ThumbnailSize)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(50, 500).WithMessage("Размер превью-изображений должен быть в предлах от 50 до 500");
    }
}
