using Microsoft.EntityFrameworkCore;
using RiseDiary.Data;
using RiseDiary.Model;
using RiseDiary.Shared;

namespace RiseDiary.WebAPI.Data;

internal static class DataSeed
{
    public async static Task CheckData(string dbFileName)
    {
        var builder = new DbContextOptionsBuilder<DiaryDbContext>();
        builder.UseSqlite($"Data Source={dbFileName};");
        using var context = new DiaryDbContext(builder.Options);

        var fileExists = File.Exists(dbFileName);

        await context.Database.MigrateAsync();

        if (fileExists) return;

        context.AppSettings.Add(new AppSetting
        {
            Key = AppSettingsKey.AvailableImagesPageSize.ToString(),
            Value = "5",
            ModifiedDate = DateTime.UtcNow
        });

        context.AppSettings.Add(new AppSetting
        {
            Key = AppSettingsKey.RecordsPageSize.ToString(),
            Value = "5",
            ModifiedDate = DateTime.UtcNow
        });

        context.AppSettings.Add(new AppSetting
        {
            Key = AppSettingsKey.ImagesPageSize.ToString(),
            Value = "10",
            ModifiedDate = DateTime.UtcNow
        });

        context.AppSettings.Add(new AppSetting
        {
            Key = AppSettingsKey.HostAndPort.ToString(),
            Value = "https://risediary.azurewebsites.net",
            ModifiedDate = DateTime.UtcNow
        });

        context.AppSettings.Add(new AppSetting
        {
            Key = AppSettingsKey.ThumbnailSize.ToString(),
            Value = "140",
            ModifiedDate = DateTime.UtcNow
        });

        context.AppSettings.Add(new AppSetting
        {
            Key = AppSettingsKey.ImageQuality.ToString(),
            Value = "75",
            ModifiedDate = DateTime.UtcNow
        });

        context.AppSettings.Add(new AppSetting
        {
            Key = AppSettingsKey.ImportantDaysDisplayRange.ToString(),
            Value = "10",
            ModifiedDate = DateTime.UtcNow
        });

        const string scopeId = "98366e10-a24b-46f4-be0b-d11f96ff78d6";

        context.Scopes.Add(new DiaryScope
        {
            Id = Guid.Parse(scopeId),
            ScopeName = "Важные даты",
            Description = "Группа записей, даты которых будут выводиться в разделе \"Даты\"",
            Themes = new List<DiaryTheme>
            {
                new DiaryTheme
                {
                    Id = Guid.NewGuid(),
                    ScopeId = Guid.Parse(scopeId),
                    ThemeName = "День рожденья"
                },
                new DiaryTheme
                {
                    Id = Guid.NewGuid(),
                    ScopeId = Guid.Parse(scopeId),
                    ThemeName = "Календарный праздник"
                }
            }
        });

        await context.SaveChangesAsync();
    }
}
