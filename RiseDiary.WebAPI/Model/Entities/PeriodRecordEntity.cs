using RiseDiary.Model;

namespace RiseDiary.WebAPI.Model.Entities;

public sealed class PeriodRecordEntity : IDeletableEntity
{
    public Guid PeriodId { get; set; }
    public Guid RecordId { get; set; }
    public PeriodEntity Period { get; set; } = null!;
    public RecordEntity Record { get; set; } = null!;
    public bool Deleted { get; set; }
}