using System.Text.Json.Serialization;

namespace RiseDiary.Common.Settings;
// Все поля должны совпадать с полями сущностей

public interface ISettingsDto
{
}

public class BookmarksSettingsDto : ISettingsDto
{
    public Guid[] Records { get; set; } = [];
    public DateTime ModifiedDateUtc { get; set; }
}

public class ImagesSettingsDto : ISettingsDto
{
    public int ImageQuality { get; set; }
    public int ThumbnailSize { get; set; }
    public DateTime ModifiedDateUtc { get; set; }
}

public class ImportantDaysSettingsDto : ISettingsDto
{
    public Guid? ImportantDaysScopeId { get; set; }
    public int ImportantDaysDisplayRange { get; set; }
    public DateTime ModifiedDateUtc { get; set; }
}

public class PagesSizesSettingsDto : ISettingsDto
{
    public int ImagesPageSize { get; set; }
    public int RecordsPageSize { get; set; }
    public int AvailableImagesPageSize { get; set; }
    public DateTime ModifiedDateUtc { get; set; }
}

