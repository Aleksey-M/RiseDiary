using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RiseDiary.Domain.Repositories;
using RiseDiary.Data.SqliteStorages;
using RiseDiary.WebUI.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace RiseDiary.WebUI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            var fName = Configuration.GetValue<string>("DataBaseFileName");
            fName = string.IsNullOrWhiteSpace(fName) ? "DefaultName" : fName;
            var path = Configuration.GetValue<string>("DataBaseFilePath");
            path = string.IsNullOrWhiteSpace(path) ? Environment.CurrentDirectory : path;
            DailyBackups.BackupFile(path, fName);
            services.AddScoped<IRepositoriesFactory, RepositoriesFactory>(sp => new RepositoriesFactory(path, fName));

            services.AddDbContext<DiaryDbContext>(options => options.UseSqlite($"Data Source={Path.Combine(path, fName)};"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }
    }
}
