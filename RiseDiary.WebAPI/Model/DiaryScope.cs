namespace RiseDiary.Model;

/// <summary>
/// Увлечения в общем смысле, или область интересов
/// </summary>
public sealed class DiaryScope : IDeletedEntity
{
    public Guid Id { get; set; }

    public string ScopeName { get; set; } = "";

    public bool Deleted { get; set; }

    public string Description { get; set; } = "";

    public ICollection<DiaryTheme> Themes { get; set; } = null!;
}

/// <summary>
/// Отдельная тема из какой-то области интересов
/// </summary>
public sealed class DiaryTheme : IDeletedEntity
{
    public Guid Id { get; set; }

    public Guid ScopeId { get; set; }

    public string ThemeName { get; set; } = "";

    public bool Actual { get; set; }

    public bool Deleted { get; set; }

    public string Description { get; set; } = "";

    public DiaryScope? Scope { get; set; }

    public ICollection<DiaryRecordTheme> RecordsRefs { get; set; } = null!;
}
