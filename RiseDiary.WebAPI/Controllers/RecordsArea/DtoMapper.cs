using RiseDiary.Common.Records;
using RiseDiary.Common.Scopes;
using RiseDiary.Model;
using RiseDiary.WebAPI.Controllers.ImagesArea;
using RiseDiary.WebAPI.Controllers.ScopesArea;

namespace RiseDiary.WebAPI.Controllers.RecordsArea;

internal static class DtoMapper
{
    public static RecordDto ToDto(this RecordEntity record) => new RecordDto
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

    public static CogitationDto ToDto(this RecordCommentEntity cogitation) => new()
    {
        Id = cogitation.Id,
        CreateDate = cogitation.Date,
        Text = cogitation.Text
    };

    public static RecordListItemDto ToListDto(this RecordEntity record) => new()
    {
        Date = record.Date,
        CreatedDate = record.CreateDate,
        ModifiedDate = record.ModifyDate,
        Name = record.Name.Length == 0 ? "[Пусто]" : record.Name,
        RecordId = record.Id
    };

    public static RecordEditDto ToEditDto(this RecordEntity record, Guid[] startPageRecords,
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
            StartPageRecords = startPageRecords,
            AllScopes = allScopes,
            AddImagesPageSize = addImagesPageSize
        };
}
