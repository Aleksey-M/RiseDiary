using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace RiseDiary.WebUI.Data
{/*
    public class DiaryDbContextFactory : IDesignTimeDbContextFactory<DiaryDbContext>
    {
        public DiaryDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            var builder = new DbContextOptionsBuilder<DiaryDbContext>();

            var fName = configuration.GetValue<string>("DataBaseFileName");
            fName = string.IsNullOrWhiteSpace(fName) ? "DefaultName" : fName;
            var path = configuration.GetValue<string>("DataBaseFilePath");
            path = string.IsNullOrWhiteSpace(path) ? Environment.CurrentDirectory : path;

            builder.UseSqlite($"Data Source={Path.Combine(path, fName)};");

            return new DiaryDbContext(builder.Options);
        }
    }*/
}
