using RiseDiary.Model;
using RiseDiary.Shared.Records;
using RiseDiary.Shared.Scopes;
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
            .OrderByDescending(c => c.CreateDate)
            .ToList(),
        Themes = record.ThemesRefs
            .Select(rt => rt.Theme.ToDto())
            .ToList(),
        Images = record.ImagesRefs
            .OrderBy(x => x.Order)
            .Select(ri => ri.Image.ToListDto())
            .ToList()
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

    public static RecordEditDto ToEditDto(this DiaryRecord record, Guid? startPageRecordId,
        List<ScopeDto> allScopes, int addImagesPageSize) => new RecordEditDto
        {
            RecordId = record.Id,
            Date = record.Date,
            CreatedDate = record.CreateDate,
            ModifiedDate = record.ModifyDate,
            Name = record.Name,
            Text = record.Text,
            Cogitations = record.Cogitations
                .Select(c => c.ToDto())
                .OrderByDescending(c => c.CreateDate)
                .ToList(),
            Themes = record.ThemesRefs
                .Select(rt => rt.Theme.ToDto())
                .ToList(),
            Images = record.ImagesRefs
                .OrderBy(x => x.Order)
                .Select(ri => ri.ToListDto())
                .ToList(),
            StartPageRecordId = startPageRecordId,
            AllScopes = allScopes,
            AddImagesPageSize = addImagesPageSize
        };
}
