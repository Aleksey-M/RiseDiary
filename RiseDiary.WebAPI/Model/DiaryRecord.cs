namespace RiseDiary.Model;

public sealed class DiaryRecord : IDeletedEntity
{
    public Guid Id { get; set; }

    public DateOnly Date { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime ModifyDate { get; set; }

    public string Name { get; set; } = "";

    public string Text { get; set; } = "";

    public bool Deleted { get; set; }

    public ICollection<Cogitation> Cogitations { get; private set; } = null!;

    public ICollection<DiaryRecordTheme> ThemesRefs { get; private set; } = null!;

    public ICollection<DiaryRecordImage> ImagesRefs { get; private set; } = null!;
}


public sealed class Cogitation : IDeletedEntity
{
    public Guid Id { get; set; }

    public Guid RecordId { get; set; }

    public DateTime Date { get; set; }

    public string Text { get; set; } = "";

    public bool Deleted { get; set; }

    public DiaryRecord? Record { get; set; }
}


public sealed class DiaryRecordTheme : IDeletedEntity
{
    public Guid ThemeId { get; set; }

    public Guid RecordId { get; set; }

    public bool Deleted { get; set; }

    public DiaryTheme? Theme { get; set; }

    public DiaryRecord? Record { get; set; }
}


public sealed class DiaryRecordImage : IDeletedEntity
{
    public Guid ImageId { get; set; }

    public Guid RecordId { get; set; }

    public bool Deleted { get; set; }

    public int Order { get; set; }

    public DiaryImage? Image { get; set; }

    public DiaryRecord? Record { get; set; }
}
