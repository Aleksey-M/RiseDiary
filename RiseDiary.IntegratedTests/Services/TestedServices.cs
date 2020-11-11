using RiseDiary.IntegratedTests.Stubs;
using RiseDiary.Model;
using RiseDiary.Model.Services;
using RiseDiary.WebUI.Data;

#pragma warning disable CA1822 // Mark members as static

namespace RiseDiary.IntegratedTests.Services
{
    internal class TestedServices : TestFixtureBase
    {
        protected ICalendarService GetCalendarService(DiaryDbContext? context = null) => new CalendarService(context ?? CreateContext(), new HostAndPortStub());
        protected IDatesService GetDatesService(int daysRange, DiaryDbContext? context = null) => new DatesService(context ?? CreateContext(), new AppSettingsForDatesServiceStub(daysRange), new HostAndPortStub());
        protected IRecordsSearchTextService GetRecordsSearchTextService(DiaryDbContext? context = null) => new RecordsSearchTextService(context ?? CreateContext(), new HostAndPortStub());
        protected IRecordsSearchService GetRecordsSearchService(DiaryDbContext? context = null) => new RecordsSearchService(context ?? CreateContext(), new HostAndPortStub());
        protected IRecordsService GetRecordsService(DiaryDbContext? context = null) => new RecordsService(context ?? CreateContext(), new HostAndPortStub());
        protected ICropImageService GetCropImageService(DiaryDbContext? context = null) => new CropImageService(context ?? CreateContext(), new AppSettingsServiceStub());
        protected IImagesEditService GetImagesEditService(DiaryDbContext? context = null) => new ImagesEditService(context ?? CreateContext(), new AppSettingsServiceStub());
        protected IImagesService GetImagesService(DiaryDbContext? context = null) => new ImagesService(context ?? CreateContext(), new AppSettingsServiceStub());
        protected IRecordsImagesService GetRecordsImagesService(DiaryDbContext? context = null) => new RecordsImagesService(context ?? CreateContext());
        protected IAppSettingsService GetAppSettingsService(DiaryDbContext? context = null) => new AppSettingsService(context ?? CreateContext());
        protected IRecordsThemesService GetRecordsThemesService(DiaryDbContext? context = null) => new RecordsThemesService(context ?? CreateContext());
        protected IScopesService GetScopesService(DiaryDbContext? context = null) => new ScopesService(context ?? CreateContext());
    }
}
