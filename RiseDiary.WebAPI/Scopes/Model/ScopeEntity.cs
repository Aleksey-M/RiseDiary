using RiseDiary.Model;

namespace RiseDiary.WebAPI.Scopes.Model;

/// <summary>
/// Увлечения в общем смысле, или область интересов
/// </summary>
public sealed class ScopeEntity : IDeletableEntity
{
    public Guid Id { get; set; }
    public string ScopeName { get; set; } = "";
    public bool Deleted { get; set; }
    public string Description { get; set; } = "";
    public ICollection<ThemeEntity> Themes { get; set; } = null!;
    public DateTime CreateDate { get; set; }
    public DateTime ModifyDate { get; set; }
}
