using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using RiseDiary.WebAPI.Settings;
using RiseDiary.WebAPI.Settings.Model;
using RiseDiary.WebAPI.Settings.Services;
using System;
using System.Threading.Tasks;

namespace RiseDiary.IntegratedTests.Services;

internal class SettingsServiceTests : TestFixtureBase
{

    [Test]
    public async Task GetBookmarksSettings_Returns_Default_When_No_Data()
    {
        var provider = BuildServiceProvider();
        var svc = provider.GetRequiredService<ISettingsService>();

        var result = await svc.GetSetting<BookmarksSettings>();

        Assert.IsTrue(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data!.ModifiedDateUtc);
        Assert.IsEmpty(result.Data!.Records);
        Assert.AreEqual(DateTime.MinValue, result.Data!.ModifiedDateUtc);
    }

    [Test]
    public async Task UpdateBookmarksSettings_Then_GetSetting_Returns_Stored_Value()
    {
        var provider = BuildServiceProvider();
        var svc = provider.GetRequiredService<ISettingsService>();

        var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var settings = new BookmarksSettings(ids, DateTime.UtcNow);

        var update = await svc.UpdateSetting(settings);
        Assert.IsTrue(update.Succeeded);

        var read = await svc.GetSetting<BookmarksSettings>();
        Assert.IsTrue(read.Succeeded);
        CollectionAssert.AreEquivalent(ids, read.Data!.Records);
    }

    [Test]
    public async Task GetImagesSettings_Returns_Defaults_When_No_Data()
    {
        var provider = BuildServiceProvider();
        var svc = provider.GetRequiredService<ISettingsService>();

        var result = await svc.GetSetting<ImagesSettings>();

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(80, result.Data!.ImageQuality);
        Assert.AreEqual(140, result.Data!.ThumbnailSize);
    }

    [Test]
    public async Task UpdateImagesSettings_And_Read_Back()
    {
        var provider = BuildServiceProvider();
        var svc = provider.GetRequiredService<ISettingsService>();

        var settings = new ImagesSettings(95, 200, DateTime.UtcNow);

        var update = await svc.UpdateSetting(settings);
        Assert.IsTrue(update.Succeeded);

        var read = await svc.GetSetting<ImagesSettings>();
        Assert.AreEqual(95, read.Data!.ImageQuality);
        Assert.AreEqual(200, read.Data!.ThumbnailSize);
    }

    [Test]
    public async Task GetImportantDaysSettings_Defaults_When_Empty()
    {
        var provider = BuildServiceProvider();
        var svc = provider.GetRequiredService<ISettingsService>();

        var result = await svc.GetSetting<ImportantDaysSettings>();

        Assert.IsTrue(result.Succeeded);
        Assert.IsNull(result.Data!.ImportantDaysScopeId);
        Assert.AreEqual(10, result.Data!.ImportantDaysDisplayRange);
    }

    [Test]
    public async Task UpdateImportantDaysSettings_And_Read_Back()
    {
        var provider = BuildServiceProvider();
        var svc = provider.GetRequiredService<ISettingsService>();

        var id = Guid.NewGuid();
        var settings = new ImportantDaysSettings(id, 30, DateTime.UtcNow);

        var update = await svc.UpdateSetting(settings);
        Assert.IsTrue(update.Succeeded);

        var read = await svc.GetSetting<ImportantDaysSettings>();
        Assert.AreEqual(id, read.Data!.ImportantDaysScopeId);
        Assert.AreEqual(30, read.Data!.ImportantDaysDisplayRange);
    }

    [Test]
    public async Task GetPagesSizesSettings_Defaults_When_Empty()
    {
        var provider = BuildServiceProvider();
        var svc = provider.GetRequiredService<ISettingsService>();

        var result = await svc.GetSetting<PagesSizesSettings>();

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(20, result.Data!.ImagesPageSize);
        Assert.AreEqual(50, result.Data!.RecordsPageSize);
        Assert.AreEqual(20, result.Data!.AvailableImagesPageSize);
    }

    [Test]
    public async Task UpdatePagesSizesSettings_And_Read_Back()
    {
        var provider = BuildServiceProvider();
        var svc = provider.GetRequiredService<ISettingsService>();

        var settings = new PagesSizesSettings(100, 200, 300, DateTime.UtcNow);

        var update = await svc.UpdateSetting(settings);
        Assert.IsTrue(update.Succeeded);

        var read = await svc.GetSetting<PagesSizesSettings>();
        Assert.AreEqual(100, read.Data!.ImagesPageSize);
        Assert.AreEqual(200, read.Data!.RecordsPageSize);
        Assert.AreEqual(300, read.Data!.AvailableImagesPageSize);
    }

    [Test]
    public async Task UpdateSetting_Fails_When_No_Storage_Registered()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<ISettingsService, SettingsService>();

        var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<ISettingsService>();

        var settings = new BookmarksSettings([], DateTime.UtcNow);

        var result = await svc.UpdateSetting(settings);

        Assert.IsFalse(result.Succeeded);
        Assert.IsNotNull(result.Message);
        StringAssert.Contains("ISettingsStorage", result.Message!);
    }
}
