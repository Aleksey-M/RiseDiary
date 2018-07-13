using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RiseDiary.WebUI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.WebUI.Migrations;

namespace RiseDiary.WebUI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private string _dataBaseFileName;
        private bool _needFileBackup;
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            _dataBaseFileName = Configuration.GetValue<string>("dbFile");
            _needFileBackup = Configuration.GetValue<int>("needFileBackup") > 0;
            if (_needFileBackup)
            {
                SqliteFileBackup.BackupFile(_dataBaseFileName);
            }            

            services.AddDbContext<DiaryDbContext>(options => options.UseSqlite($"Data Source={_dataBaseFileName};"));

            var builder = new DbContextOptionsBuilder<DiaryDbContext>();
            builder.UseSqlite($"Data Source={_dataBaseFileName};");

            int needMigration = Configuration.GetValue<int>("needMigration");
            if (needMigration > 0)
            {
                var context = new DiaryDbContext(builder.Options);
                context.Database.Migrate();
            }            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {            
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseMvc();

            if (_needFileBackup)
            {
                applicationLifetime.ApplicationStopped.Register(() => SqliteFileBackup.BackupFile(_dataBaseFileName));
            }
        }
    }
}
