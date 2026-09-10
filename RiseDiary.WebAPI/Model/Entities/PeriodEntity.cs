using RiseDiary.Model;

namespace RiseDiary.WebAPI.Model.Entities;

public sealed class PeriodEntity : IDeletableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime? Finish { get; set; }
    public ICollection<PeriodRecordEntity> PeriodRecords { get; set; } = null!;
    public bool Deleted { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime ModifyDate { get; set; }
}
