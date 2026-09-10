using RiseDiary.Model;

namespace RiseDiary.WebAPI.Scopes.Model;

/// <summary>
/// Отдельная тема из какой-то области интересов
/// </summary>
public sealed class ThemeEntity : IDeletableEntity
{
    public Guid Id { get; set; }
    public Guid ScopeId { get; set; }
    public string ThemeName { get; set; } = "";
    public bool Actual { get; set; }
    public bool Deleted { get; set; }
    public string Description { get; set; } = "";
    public ScopeEntity? Scope { get; set; }
    public ICollection<RecordThemeEntity> RecordsRefs { get; set; } = null!;
    public DateTime CreateDate { get; set; }
    public DateTime ModifyDate { get; set; }
}
