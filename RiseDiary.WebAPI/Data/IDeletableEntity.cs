namespace RiseDiary.WebAPI.Data;

public interface IDeletableEntity
{
    bool Deleted { get; set; }
}
