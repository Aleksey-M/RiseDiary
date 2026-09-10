namespace RiseDiary.Model;


/// <summary>
/// Запись дневника
/// </summary>
public sealed class RecordEntity : IDeletableEntity
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime ModifyDate { get; set; }
    public string Name { get; set; } = "";
    public string Text { get; set; } = "";
    public bool Deleted { get; set; }
    public ICollection<RecordCommentEntity> Cogitations { get; private set; } = null!;
    public ICollection<RecordThemeEntity> ThemesRefs { get; private set; } = null!;
    public ICollection<RecordImageEntity> ImagesRefs { get; private set; } = null!;
}
