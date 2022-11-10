namespace RiseDiary.WebAPI.Config;

public class SqliteOptions
{
    public const string SectionName = "SqliteOptions";

    public string FileName { get; set; } = string.Empty;
}
