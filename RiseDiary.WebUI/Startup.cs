using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RiseDiary.Model;
using RiseDiary.Model.Services;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private readonly IConfiguration _configuration;
        private string _dataBaseFileName = "";
        private bool _needFileBackup;

        public void ConfigureServices(IServiceCollection services)
        {
            _dataBaseFileName = _configuration.GetValue<string>("dbFile");
            _needFileBackup = _configuration.GetValue<int>("needFileBackup") > 0;
            if (_needFileBackup)
            {
                SqliteFileBackup.BackupFile(_dataBaseFileName);
            }

            services.AddRazorPages();

            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDbContext<DiaryDbContext>(options => options.UseSqlite($"Data Source={_dataBaseFileName};"));
            services.AddTransient<IScopesService, ScopesService>();
            services.AddTransient<IAppSettingsService, AppSettingsService>();
            services.AddTransient<IRecordsThemesService, RecordsThemesService>();
            services.AddTransient<IImagesService, ImagesService>();
            services.AddTransient<IRecordsImagesService, RecordsImagesService>();
            services.AddTransient<IImagesEditService, ImagesEditService>();
            services.AddTransient<ICropImageService, CropImageService>();
            services.AddTransient<IHostAndPortService, HostAndPortService>();
            services.AddTransient<IRecordsService, RecordsService>();
            services.AddTransient<IRecordsSearchService, RecordsSearchService>();
            services.AddTransient<IRecordsSearchTextService, RecordsSearchTextService>();
            services.AddTransient<IDatesService, DatesService>();
            services.AddTransient<ICalendarService, CalendarService>();
            services.AddTransient<ISqliteDatabase, SqliteDatabase>();

            services.AddServerSideBlazor();
            services.AddSwaggerDocument();
            services.AddMvcCore().AddApiExplorer();

            int needMigration = _configuration.GetValue<int>("needMigration");
            if (needMigration > 0)
            {
                var builder = new DbContextOptionsBuilder<DiaryDbContext>();
                builder.UseSqlite($"Data Source={_dataBaseFileName};");
                using var context = new DiaryDbContext(builder.Options);
                context.Database.Migrate();
            }
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime)
        {
            app.UseDeveloperExceptionPage();
            app.UseHsts();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapBlazorHub();
            });

            app.UseOpenApi();
            app.UseSwaggerUi3();

            if (_needFileBackup && applicationLifetime != null)
            {
                applicationLifetime.ApplicationStopped.Register(() => SqliteFileBackup.BackupFile(_dataBaseFileName));
            }
        }
    }
}
