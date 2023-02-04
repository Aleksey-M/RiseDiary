using RiseDiary.Data;
using RiseDiary.Model;
using RiseDiary.Model.Services;

namespace RiseDiary.IntegratedTests.Services;

// плохая практика
public static class TestedServices
{
    static public ICalendarService GetCalendarService(DiaryDbContext context) => new CalendarService(context);

    static public IDatesService GetDatesService(DiaryDbContext context, IAppSettingsService appSettingsService) => new DatesService(context, appSettingsService);

    static public IRecordsSearchTextService GetRecordsSearchTextService(DiaryDbContext context, IAppSettingsService appSettingsService) => new RecordsSearchTextService(context, appSettingsService);

    static public IRecordsSearchService GetRecordsSearchService(DiaryDbContext context, IAppSettingsService appSettingsService) => new RecordsSearchService(context, appSettingsService);

    static public IRecordsService GetRecordsService(DiaryDbContext context) => new RecordsService(context);

    static public IImagesEditService GetImagesEditService(DiaryDbContext context, IAppSettingsService appSettingsService) => new ImagesEditService(context, appSettingsService);

    static public IImagesService GetImagesService(DiaryDbContext context, IAppSettingsService appSettingsService) => new ImagesService(context, appSettingsService);

    static public IRecordsImagesService GetRecordsImagesService(DiaryDbContext context) => new RecordsImagesService(context);

    static public IAppSettingsService GetAppSettingsService(DiaryDbContext context) => new AppSettingsService(context);

    static public IRecordsThemesService GetRecordsThemesService(DiaryDbContext context) => new RecordsThemesService(context);

    static public IScopesService GetScopesService(DiaryDbContext context) => new ScopesService(context);

    static public ICogitationsService GetCogitationsService(DiaryDbContext context) => new CogitationsService(context);
}
