using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using RiseDiary.Data;

namespace Migrations;

[TestFixture]
public class OneTimeMigrations
{
    [Test]
    [Ignore("One time migration")]
    public async Task RecreateDb()
    {
        var sourceFullName = @"D:\DB\Diary (3).db";
        var destFullName = @"D:\DB\DiaryDB.new.db";

        var sourceDb = CreateContext(sourceFullName);
        await sourceDb.Database.MigrateAsync();

        var destDb = CreateContext(destFullName);
        await destDb.Database.MigrateAsync();

        var scopes = await sourceDb.Scopes
            .AsNoTracking()
            .Include(x => x.Themes)
            .ToListAsync();

        destDb.Scopes.AddRange(scopes);

        var images = await sourceDb.Images
            .AsNoTracking()
            .Include(x => x.FullImage)
            .ToListAsync();

        images.ForEach(x => x.ContentType = @"image/jpeg");

        destDb.Images.AddRange(images);

        var records = await sourceDb.Records
            .AsNoTracking()
            .Include(x => x.Cogitations)
            .Include(x => x.ImagesRefs)
            .Include(x => x.ThemesRefs)
            .ToListAsync();

        destDb.Records.AddRange(records);

        await destDb.SaveChangesAsync();

        static DiaryDbContext CreateContext(string fileName)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder().Build();
            var builder = new DbContextOptionsBuilder<DiaryDbContext>();
            builder.UseSqlite($"Data Source={fileName};", o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            return new DiaryDbContext(builder.Options);
        }
    }

    [Test, Ignore("One time migration")]
    public async Task UpdateImageLinksInText()
    {
        var sqliteFile = @"C:\Users\Oleksii.Mamikonian\source\repos\Db\DbRiseDiary.db";
        var connStr = $@"Data Source={sqliteFile};";
        IConfigurationRoot configuration = new ConfigurationBuilder().Build();
        var builder = new DbContextOptionsBuilder<DiaryDbContext>();
        builder.UseSqlite(connStr);
        using var context = new DiaryDbContext(builder.Options);
        context.Database.Migrate();

        var allRecords = context.Records.Include(r => r.Cogitations).AsAsyncEnumerable();
        await foreach (var rec in allRecords)
        {
            var newText = rec.Text.Replace(@"/Images/ImageFile/", @"/api/v1.0/image-file/", true, null);
            rec.Text = newText;

            foreach (var cog in rec.Cogitations)
            {
                var newCogText = cog.Text.Replace(@"/Images/ImageFile/", @"/api/v1.0/image-file/", true, null);
                cog.Text = newCogText;
            }
        }

        await context.SaveChangesAsync();
    }

    [Test, Ignore("One time migration")]
    public async Task UpdateDatesToDateOnly()
    {
        var fileNameFull = "D:\\Projects\\RiseDiary\\DB\\Diary.db";
        var connStrSource = $@"Data Source={fileNameFull};";

        using var connection = new SqliteConnection(connStrSource);
        connection.Open();

        var records = new List<(string, string)>();

        using var readCommand = connection.CreateCommand();
        readCommand.CommandText = "SELECT Id, Date FROM Records";
        using var reader = readCommand.ExecuteReader();
        while (await reader.ReadAsync())
        {
            records.Add((
                reader["Id"]?.ToString() ?? "",
                reader["Date"]?.ToString() ?? ""));
        }
        reader.Close();

        var updatedDates = records
            .Select(r => (Id: r.Item1, Date: r.Item2.Split(" ")[0]))
            .Select(r => $"update Records set Date = '{r.Date}' where Id = '{r.Id}'")
            .ToList();

        using var updateCommand = connection.CreateCommand();
        foreach (var updateDate in updatedDates)
        {
            updateCommand.CommandText = updateDate;
            await updateCommand.ExecuteNonQueryAsync();
        }
    }

    [Test, Ignore("One time migration")]
    public async Task MigrateToUtcDates()
    {
        var fileNameFull = "D:\\Projects\\RiseDiary\\DB\\Diary.db";
        var connStrSource = $@"Data Source={fileNameFull};";

        using var connection = new SqliteConnection(connStrSource);
        connection.Open();

        // Records
        var records = new List<(string, string, string)>();

        using var readCommand = connection.CreateCommand();
        readCommand.CommandText = "select Id, CreateDate, ModifyDate FROM Records";
        using var reader = readCommand.ExecuteReader();
        while (await reader.ReadAsync())
        {
            records.Add((
                reader["Id"]?.ToString() ?? "",
                reader["CreateDate"]?.ToString() ?? "",
                reader["ModifyDate"]?.ToString() ?? ""));
        }
        reader.Close();

        var updateRecordsSqlCommands = records
            .Select(x => (x.Item1, ConvertToUtc(x.Item2), ConvertToUtc(x.Item3)))
            .Select(x => $"update Records set CreateDate = '{x.Item2}', ModifyDate = '{x.Item3}' where Id = '{x.Item1}'")
            .ToList();

        using var updateRecordsCommand = connection.CreateCommand();
        foreach (var updateDate in updateRecordsSqlCommands)
        {
            updateRecordsCommand.CommandText = updateDate;
            await updateRecordsCommand.ExecuteNonQueryAsync();
        }

        // Cogitations
        var cogitations = new List<(string, string)>();

        var cogitationsReadCommand = connection.CreateCommand();
        cogitationsReadCommand.CommandText = "select Id, Date FROM Cogitations";
        using var cogReader = cogitationsReadCommand.ExecuteReader();
        while (await cogReader.ReadAsync())
        {
            cogitations.Add((
                cogReader["Id"]?.ToString() ?? "",
                cogReader["Date"]?.ToString() ?? ""));
        }
        cogReader.Close();

        var updateCogitationsSqlCommands = cogitations
            .Select(x => (x.Item1, ConvertToUtc(x.Item2)))
            .Select(x => $"update Cogitations set Date = '{x.Item2}' where Id = '{x.Item1}'")
            .ToList();

        using var updateCogitationsCommand = connection.CreateCommand();
        foreach (var updateDate in updateCogitationsSqlCommands)
        {
            updateCogitationsCommand.CommandText = updateDate;
            await updateCogitationsCommand.ExecuteNonQueryAsync();
        }

        // App settings
        var appSettings = new List<(string, string)>();

        var appSettingsReadCommand = connection.CreateCommand();
        appSettingsReadCommand.CommandText = "select Key, ModifiedDate from AppSettings";
        using var appSettingsReader = appSettingsReadCommand.ExecuteReader();
        while (await appSettingsReader.ReadAsync())
        {
            appSettings.Add((
                appSettingsReader["Key"]?.ToString() ?? "",
                appSettingsReader["ModifiedDate"]?.ToString() ?? ""));
        }
        appSettingsReader.Close();

        var updateAppSettingsSqlCommands = appSettings
            .Select(x => (x.Item1, ConvertToUtc(x.Item2)))
            .Select(x => $"update AppSettings set ModifiedDate = '{x.Item2}' where Key = '{x.Item1}'")
            .ToList();

        using var updateAppSettingsCommand = connection.CreateCommand();
        foreach (var updateDate in updateAppSettingsSqlCommands)
        {
            updateAppSettingsCommand.CommandText = updateDate;
            await updateAppSettingsCommand.ExecuteNonQueryAsync();
        }

        // Images
        var images = new List<(string, string, string)>();

        using var imagesReadCommand = connection.CreateCommand();
        imagesReadCommand.CommandText = "select Id, CreateDate, ModifyDate FROM Images";
        using var imagesReader = imagesReadCommand.ExecuteReader();
        while (await imagesReader.ReadAsync())
        {
            images.Add((
                imagesReader["Id"]?.ToString() ?? "",
                imagesReader["CreateDate"]?.ToString() ?? "",
                imagesReader["ModifyDate"]?.ToString() ?? ""));
        }
        imagesReader.Close();

        var updateImagessSqlCommands = images
            .Select(x => (x.Item1, ConvertToUtc(x.Item2), ConvertToUtc(x.Item3)))
            .Select(x => $"update Images set CreateDate = '{x.Item2}', ModifyDate = '{x.Item3}' where Id = '{x.Item1}'")
            .ToList();

        using var updateImagesCommand = connection.CreateCommand();
        foreach (var updateDate in updateImagessSqlCommands)
        {
            updateImagesCommand.CommandText = updateDate;
            await updateImagesCommand.ExecuteNonQueryAsync();
        }

        //
        static string ConvertToUtc(string stringDate)
        {
            var lastLocalChangesDate = new DateTime(2021, 3, 1); // после 1 марта не обновляем
            var date = DateTime.Parse(stringDate);
            if (date < lastLocalChangesDate)
            {
                date = DateTime.SpecifyKind(date, DateTimeKind.Local);
                var utcDate = TimeZoneInfo.ConvertTimeToUtc(date, TimeZoneInfo.Local);
                return utcDate.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
            }
            else
            {
                return stringDate;
            }
        }
    }
}
