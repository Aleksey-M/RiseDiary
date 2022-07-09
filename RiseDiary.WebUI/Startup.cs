using Microsoft.AspNetCore.Authentication.Cookies;
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

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(cookieOptions =>
            {
                cookieOptions.LoginPath = "/Login";
            });

            services.AddMvc().AddRazorPagesOptions(options =>
            {
                options.Conventions.AuthorizeFolder("/");
            });

            services.AddDbContext<DiaryDbContext>(options => options.UseSqlite(
                $"Data Source={_dataBaseFileName};", o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

            services.AddTransient<IScopesService, ScopesService>();
            services.AddTransient<IAppSettingsService, AppSettingsService>();
            services.AddTransient<IRecordsThemesService, RecordsThemesService>();
            services.AddTransient<IImagesService, ImagesService>();
            services.AddTransient<IRecordsImagesService, RecordsImagesService>();
            services.AddTransient<IImagesEditService, ImagesEditService>();
            services.AddTransient<ICropImageService, CropImageService>();
            services.AddTransient<IRecordsService, RecordsService>();
            services.AddTransient<ICogitationsService, CogitationsService>();
            services.AddTransient<IRecordsSearchService, RecordsSearchService>();
            services.AddTransient<IRecordsSearchTextService, RecordsSearchTextService>();
            services.AddTransient<IDatesService, DatesService>();
            services.AddTransient<ICalendarService, CalendarService>();
            services.AddTransient<ISqliteDatabase, SqliteDatabase>();

            services.AddSingleton<BrowserTimeOffsetService>();

            services.AddServerSideBlazor();

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
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapBlazorHub();
            });

            if (_needFileBackup && applicationLifetime != null)
            {
                applicationLifetime.ApplicationStopped.Register(() => SqliteFileBackup.BackupFile(_dataBaseFileName));
            }
        }
    }
}
