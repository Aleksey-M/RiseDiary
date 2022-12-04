using RiseDiary.Data;
using RiseDiary.Model;
using RiseDiary.Model.Services;

namespace RiseDiary.IntegratedTests.Services;

// плохая практика
public static class TestedServices
{
    static public ICalendarService GetCalendarService(DiaryDbContext context, IAppSettingsService appSettingsService) => new CalendarService(context, appSettingsService);

    static public IDatesService GetDatesService(DiaryDbContext context, IAppSettingsService appSettingsService) => new DatesService(context, appSettingsService);

    static public IRecordsSearchTextService GetRecordsSearchTextService(DiaryDbContext context, IAppSettingsService appSettingsService) => new RecordsSearchTextService(context, appSettingsService);

    static public IRecordsSearchService GetRecordsSearchService(DiaryDbContext context, IAppSettingsService appSettingsService) => new RecordsSearchService(context, appSettingsService);

    static public IRecordsService GetRecordsService(DiaryDbContext context, IAppSettingsService appSettingsService) => new RecordsService(context, appSettingsService);

    static public IImagesEditService GetImagesEditService(DiaryDbContext context, IAppSettingsService appSettingsService) => new ImagesEditService(context, appSettingsService);

    static public IImagesService GetImagesService(DiaryDbContext context, IAppSettingsService appSettingsService) => new ImagesService(context, appSettingsService);

    static public IRecordsImagesService GetRecordsImagesService(DiaryDbContext context) => new RecordsImagesService(context);

    static public IAppSettingsService GetAppSettingsService(DiaryDbContext context) => new AppSettingsService(context);

    static public IRecordsThemesService GetRecordsThemesService(DiaryDbContext context) => new RecordsThemesService(context);

    static public IScopesService GetScopesService(DiaryDbContext context, IAppSettingsService appSettingsService) => new ScopesService(context, appSettingsService);

    static public ICogitationsService GetCogitationsService(DiaryDbContext context, IAppSettingsService appSettingsService) => new CogitationsService(context, appSettingsService);
}
