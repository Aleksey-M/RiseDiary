using System.Globalization;

namespace RiseDiary.Shared.Images;

public static class ImageHelper
{
    public static string ToFileSizeString(this long bytes) => (bytes) switch
    {
        <= 1024 * 1024 => Math.Round(bytes / 1024f, 2).ToString(CultureInfo.InvariantCulture) + " Kb",
        _ => Math.Round(bytes / (1024f * 1024f), 2).ToString(CultureInfo.InvariantCulture) + " Mb"
    };
}
