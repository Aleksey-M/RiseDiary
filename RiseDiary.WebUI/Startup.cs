using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private string _dataBaseFileName = string.Empty;
        private bool _needFileBackup;

        public void ConfigureServices(IServiceCollection services)
        {
            _dataBaseFileName = Configuration.GetValue<string>("dbFile");
            _needFileBackup = Configuration.GetValue<int>("needFileBackup") > 0;
            if (_needFileBackup)
            {
                SqliteFileBackup.BackupFile(_dataBaseFileName);
            }

            services.AddRazorPages();
            services.AddDbContext<DiaryDbContext>(options => options.UseSqlite($"Data Source={_dataBaseFileName};"));
            services.AddServerSideBlazor();

            int needMigration = Configuration.GetValue<int>("needMigration");
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
            app.UseStaticFiles();
            //app.UseHttpsRedirection();
            //app.UseAuthorization();
            
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
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
