namespace RiseDiary.Model;

/// <summary>
/// Размышления (осмысления), добавленные к записи. Оно же "Комментарий"
/// </summary>
public sealed class RecordCommentEntity : IDeletableEntity
{
    public Guid Id { get; set; }
    public Guid RecordId { get; set; }
    public DateTime Date { get; set; }
    public string Text { get; set; } = "";
    public bool Deleted { get; set; }
    public RecordEntity? Record { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime ModifyDate { get; set; }
}
