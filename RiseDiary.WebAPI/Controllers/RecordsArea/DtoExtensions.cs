using System.Xml.Linq;
using RiseDiary.Model;
using RiseDiary.Shared.Records;
using RiseDiary.Shared.Scopes;
using RiseDiary.WebAPI.Controllers.ImagesArea;
using RiseDiary.WebAPI.Controllers.ScopesArea;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        Name = record.Name.Length == 0 ? "[Пусто]" : record.Name,
        RecordId = record.Id
    };

    public static RecordEditDto ToEditDto(this DiaryRecord record, Guid? startPageRecordId, ScopeDto[] allScopes) => new RecordEditDto
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
            .ToArray(),
        StartPageRecordId = startPageRecordId,
        AllScopes = allScopes
    };
}
