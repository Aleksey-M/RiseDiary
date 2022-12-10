using RiseDiary.Model;
using RiseDiary.Shared.Records;
using RiseDiary.WebAPI.Controllers.ImagesArea;
using RiseDiary.WebAPI.Controllers.ScopesArea;

namespace RiseDiary.WebAPI.Controllers.RecordsArea;

internal static class DtoExtensions
{
    public static RecordDto ToDto(this DiaryRecord record) => new RecordDto
    {
        RecordId = record.Id,
        Date = record.Date,
        CreatedDate = record.CreateDate,
        ModifiedDate = record.ModifyDate,
        Name = record.Name,
        Text = record.Text,
        Cogitations = record.Cogitations
            .Select(c => c.ToDto())
            .ToArray(),
        Themes = record.ThemesRefs
            .Select(rt => rt.Theme.ToDto())
            .ToArray(),
        Images = record.ImagesRefs
            .Select(ri => ri.Image.ToListDto())
            .ToArray()
    };

    public static CogitationDto ToDto(this Cogitation cogitation) => new()
    {
        Id = cogitation.Id,
        CreateDate = cogitation.Date,
        Text = cogitation.Text
    };

    public static RecordListItemDto ToListDto(this DiaryRecord record) => new()
    {
        Date = record.Date,
        CreatedDate = record.CreateDate,
        ModifiedDate = record.ModifyDate,
        Name = record.Name,
        RecordId = record.Id
    };
}
