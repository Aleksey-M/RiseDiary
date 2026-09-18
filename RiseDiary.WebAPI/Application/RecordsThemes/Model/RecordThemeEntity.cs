using RiseDiary.WebAPI.Application.Records.Model;
using RiseDiary.WebAPI.Application.Scopes.Model;
using RiseDiary.WebAPI.Data;

namespace RiseDiary.WebAPI.Application.RecordsThemes.Model;

/// <summary>
/// Сущность для привязки темы интересов к записи (связ много ко многим)
/// </summary>
public sealed class RecordThemeEntity : IDeletableEntity
{
    public Guid ThemeId { get; set; }
    public Guid RecordId { get; set; }
    public bool Deleted { get; set; }
    public ThemeEntity? Theme { get; set; }
    public RecordEntity? Record { get; set; }
}
